using System.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TestTask.Application.Interfaces;
using TestTask.Core;
using TestTask.Core.Entities;
using TestTask.Core.Exceptions;
using TestTask.Core.Extensions;
using TestTask.Core.Models;
using TestTask.Infrastructure.Data;

namespace TestTask.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly ApplicationDbContext _dbContext;

    public TransactionService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<TransactionResponse> CreditAsync(ITransaction transaction, CancellationToken token)
    {
        return await ProcessOperationAsync(transaction, (client, amount) =>
        {
            client.Balance += amount;
            return TransactionType.Credit;
        }, token);
    }

    public async Task<TransactionResponse> DebitAsync(ITransaction transaction, CancellationToken token)
    {
        return await ProcessOperationAsync(transaction, (client, amount) =>
        {
            if (client.Balance < amount)
                throw new InvalidOperationException($"Недостаточно средств TransactionId = {transaction.Id}");

            client.Balance -= amount;
            return TransactionType.Debit;
        }, token);
    }

    public async Task<RevertResponse> RevertAsync(Guid transactionId, CancellationToken token)
    {
        await using var dbTransaction =
            await _dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, token);

        var financeTransaction = await _dbContext.FinanceTransaction
            .Where(t => t.Id == transactionId)
            .Select(t => new
            {
                Transaction = t,
                CompletedHistory = t.TransactionHistories.First(th => th.Status == TransactionStatus.Completed),
                RevertedHistory = t.TransactionHistories.FirstOrDefault(th => th.Status == TransactionStatus.Reverted)
            })
            .FirstOrDefaultAsync(token);

        if (financeTransaction == null)
            throw new TransactionNotFoundException(transactionId);

        if (financeTransaction.RevertedHistory != null)
            return new RevertResponse
            {
                RevertDateTime = financeTransaction.RevertedHistory.ModificationDate,
                ClientBalance = financeTransaction.RevertedHistory.NewClientBalance
            };


        var client = await _dbContext.Clients
            .FromSqlRaw(
                "SELECT * FROM clients WHERE id = @id FOR UPDATE",
                new NpgsqlParameter("@id", financeTransaction.Transaction.ClientId))
            .FirstOrDefaultAsync(token);


        var oldClientBalance = client.Balance;

        // TODO непонятная ситуация
        if (financeTransaction.Transaction.TransactionType == TransactionType.Credit)
        {
            if (client.Balance < financeTransaction.Transaction.Amount)
                throw new InvalidOperationException(
                    $"Недостаточно средств для отмены TransactionId = {financeTransaction.Transaction.Id}");
            client.Balance -= financeTransaction.Transaction.Amount;
        }
        else
            client.Balance += financeTransaction.Transaction.Amount;


        var transactionHistory = new TransactionHistory
        {
            Id = Guid.NewGuid(),
            FinanceTransactionId = transactionId,
            Status = TransactionStatus.Reverted,
            ModificationDate = DateTime.UtcNow.ToUnspecified(),
            OldClientBalance = oldClientBalance,
            NewClientBalance = client.Balance
        };

        _dbContext.TransactionHistory.Add(transactionHistory);

        await _dbContext.SaveChangesAsync(token);
        await dbTransaction.CommitAsync(token);


        return new RevertResponse
        {
            RevertDateTime = transactionHistory.ModificationDate, ClientBalance = transactionHistory.NewClientBalance
        };
    }

    public async Task<BalanceResponse> GetBalanceAsync(Guid clientId, CancellationToken token)
    {
        var client = await _dbContext.Clients.AsNoTracking().FirstOrDefaultAsync(cl => cl.Id == clientId, token);
        if (client == null) throw new ClientNotFoundException(clientId);
        return new BalanceResponse
            { BalanceDateTime = DateTime.UtcNow.ToUnspecified(), ClientBalance = client.Balance };
    }


    private async Task<TransactionResponse> ProcessOperationAsync(ITransaction transaction,
        Func<Client, decimal, TransactionType> process, CancellationToken token)
    {
        await using var dbTransaction =
            await _dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, token);

        var existingTransaction = await _dbContext.FinanceTransaction
            .Where(t => t.Id == transaction.Id)
            .Select(t => new
            {
                Transaction = t,
                History = t.TransactionHistories.First(th => th.Status == TransactionStatus.Completed)
            })
            .FirstOrDefaultAsync(token);

        if (existingTransaction != null)
            return new TransactionResponse
            {
                InsertDateTime = existingTransaction.History.ModificationDate,
                ClientBalance = existingTransaction.History.NewClientBalance
            };

        var client = await _dbContext.Clients
            .FromSqlRaw("SELECT * FROM clients WHERE id = @id FOR UPDATE",
                new NpgsqlParameter("@id", transaction.ClientId))
            .FirstOrDefaultAsync(token);

        var oldClientBalance = client.Balance;
        var transactionType = process.Invoke(client, transaction.Amount);

        var financeTransaction = new FinanceTransaction
        {
            Id = transaction.Id,
            ClientId = transaction.ClientId,
            DateTime = transaction.DateTime.ToUniversalTime().ToUnspecified(),
            Amount = transaction.Amount,
            TransactionType = transactionType,
        };

        var transactionHistory = new TransactionHistory
        {
            Id = Guid.NewGuid(),
            FinanceTransactionId = transaction.Id,
            Status = TransactionStatus.Completed,
            ModificationDate = DateTime.UtcNow.ToUnspecified(),
            OldClientBalance = oldClientBalance,
            NewClientBalance = client.Balance
        };

        _dbContext.FinanceTransaction.Add(financeTransaction);
        _dbContext.TransactionHistory.Add(transactionHistory);

        await _dbContext.SaveChangesAsync(token);
        await dbTransaction.CommitAsync(token);

        return new TransactionResponse
        {
            InsertDateTime = transactionHistory.ModificationDate, ClientBalance = transactionHistory.NewClientBalance
        };
    }
}