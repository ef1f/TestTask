using System.Data;
using Microsoft.EntityFrameworkCore;
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
        var financeTransaction = await _dbContext.Transactions.FirstOrDefaultAsync(t => t.Id == transactionId, token);
        if (financeTransaction == null)
            throw new TransactionNotFoundException(transactionId);

        if (financeTransaction.Status == TransactionStatus.Completed)
        {
            var client = await _dbContext.Clients
                .FromSqlInterpolated($"SELECT * FROM clients WHERE id = {financeTransaction.ClientId} FOR UPDATE")
                .FirstOrDefaultAsync(token);

            client.Balance += (financeTransaction.TransactionType == TransactionType.Credit
                ? -financeTransaction.Amount
                : financeTransaction.Amount);

            financeTransaction.Status = TransactionStatus.Reverted;
            financeTransaction.RevertedAt = DateTime.UtcNow.ToUnspecified();
            financeTransaction.ClientBalance = client.Balance;

            await _dbContext.SaveChangesAsync(token);
            await dbTransaction.CommitAsync(token);
        }

        return new RevertResponse
            { RevertDateTime = financeTransaction.RevertedAt.Value, ClientBalance = financeTransaction.ClientBalance };
    }

    public async Task<BalanceResponse> GetBalanceAsync(Guid clientId, CancellationToken token)
    {
        var client = await _dbContext.Clients.AsNoTracking().FirstOrDefaultAsync(cl => cl.Id == clientId, token);
        if (client == null) throw new ClientNotFoundException(clientId);
        return new BalanceResponse { BalanceDateTime = DateTime.UtcNow.ToUnspecified(), ClientBalance = client.Balance };
    }


    private async Task<TransactionResponse> ProcessOperationAsync(ITransaction transaction,
        Func<Client, decimal, TransactionType> process, CancellationToken token)
    {
        await using var dbTransaction =
            await _dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, token);

        var existingTransaction = await _dbContext.Transactions.FindAsync(transaction.Id, token);
        if (existingTransaction != null)
            return new TransactionResponse
                { InsertDateTime = existingTransaction.InsertedAt, ClientBalance = existingTransaction.ClientBalance };

        var client = await _dbContext.Clients
            .FromSqlInterpolated($"SELECT * FROM clients WHERE id = {transaction.ClientId} FOR UPDATE")
            .FirstOrDefaultAsync(token);

        var transactionType = process.Invoke(client, transaction.Amount);

        var financeTransaction = new FinanceTransaction
        {
            Id = transaction.Id,
            ClientId = transaction.ClientId,
            DateTime = transaction.DateTime.ToUniversalTime().ToUnspecified(),
            Amount = transaction.Amount,
            TransactionType = transactionType,
            Status = TransactionStatus.Completed,
            InsertedAt = DateTime.UtcNow.ToUnspecified(),
            ClientBalance = client.Balance
        };

        _dbContext.Transactions.Add(financeTransaction);
        await _dbContext.SaveChangesAsync(token);
        await dbTransaction.CommitAsync(token);

        return new TransactionResponse
            { InsertDateTime = financeTransaction.InsertedAt, ClientBalance = financeTransaction.ClientBalance };
    }
}