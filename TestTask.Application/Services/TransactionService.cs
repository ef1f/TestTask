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
    private readonly Dictionary<TransactionType, ITransactionStrategy> _strategies;
    private readonly IClientSpecificRepository _clientSpecificRepository;
    
    public TransactionService(ApplicationDbContext dbContext,
        IClientSpecificRepository clientRepository,
        IEnumerable<ITransactionStrategy> strategies)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _strategies = strategies.ToDictionary(s => s.Type);
        _clientSpecificRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
    }

    public async Task<TransactionResponse> CreditAsync(ITransaction transaction, CancellationToken token)
        => await ExecuteOperationAsync(transaction, TransactionType.Credit, token);

    public async Task<TransactionResponse> DebitAsync(ITransaction transaction, CancellationToken token)
        => await ExecuteOperationAsync(transaction, TransactionType.Debit, token);

    public async Task<RevertResponse> RevertAsync(Guid transactionId, CancellationToken token)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            var data = await GetTransactionWithHistoryAsync(transactionId, token);
            if (data.RevertedHistory != null)
                return new RevertResponse
                {
                    RevertDateTime = data.RevertedHistory.ModificationDate,
                    ClientBalance = data.RevertedHistory.NewClientBalance
                };

            var client = await _clientSpecificRepository.GetClientForUpdateAsync(data.Transaction.ClientId, token);
            var strategy = GetStrategy(data.Transaction.TransactionType);

            var oldBalance = client.Balance;
            strategy.Revert(client, data.Transaction.Amount);

            var history = CreateHistoryRecord(transactionId, TransactionStatus.Reverted, oldBalance, client.Balance);
            _dbContext.TransactionHistory.Add(history);

            await _dbContext.SaveChangesAsync(token);
            return new RevertResponse
            { RevertDateTime = history.ModificationDate, ClientBalance = history.NewClientBalance };
        }, token);
    }

    public async Task<BalanceResponse> GetBalanceAsync(Guid clientId, CancellationToken token)
    {
        var client = await _dbContext.Clients.AsNoTracking()
            .FirstOrDefaultAsync(cl => cl.Id == clientId, token);

        if (client == null) throw new ClientNotFoundException(clientId);

        return new BalanceResponse
        {
            BalanceDateTime = DateTime.UtcNow.ToUnspecified(),
            ClientBalance = client.Balance
        };
    }

    private async Task<TransactionResponse> ExecuteOperationAsync(ITransaction transaction, TransactionType type,
        CancellationToken token)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            var existing = await _dbContext.TransactionHistory
                .Where(th => th.FinanceTransactionId == transaction.Id && th.Status == TransactionStatus.Completed)
                .Select(th => new TransactionResponse
                { InsertDateTime = th.ModificationDate, ClientBalance = th.NewClientBalance })
                .FirstOrDefaultAsync(token);

            if (existing != null) return existing;

            var client = await _clientSpecificRepository.GetClientForUpdateAsync(transaction.ClientId, token);
            var strategy = GetStrategy(type);

            var oldBalance = client.Balance;
            strategy.Apply(client, transaction.Amount);

            var financeTx = new FinanceTransaction
            {
                Id = transaction.Id,
                ClientId = transaction.ClientId,
                DateTime = transaction.DateTime.ToUniversalTime().ToUnspecified(),
                Amount = transaction.Amount,
                TransactionType = type
            };

            var history = CreateHistoryRecord(transaction.Id, TransactionStatus.Completed, oldBalance, client.Balance);

            _dbContext.FinanceTransaction.Add(financeTx);
            _dbContext.TransactionHistory.Add(history);

            await _dbContext.SaveChangesAsync(token);
            return new TransactionResponse
            { InsertDateTime = history.ModificationDate, ClientBalance = history.NewClientBalance };
        }, token);
    }

    private async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken token)
    {
        await using var dbTransaction =
            await _dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, token);

        var result = await action();

        await dbTransaction.CommitAsync(token);
        return result;
    }

    private ITransactionStrategy GetStrategy(TransactionType type)
        => _strategies.TryGetValue(type, out var strategy)
            ? strategy
            : throw new NotSupportedException($"Тип {type} не поддерживается");

    private TransactionHistory
        CreateHistoryRecord(Guid txId, TransactionStatus status, decimal oldBal, decimal newBal) => new()
        {
            Id = Guid.NewGuid(),
            FinanceTransactionId = txId,
            Status = status,
            ModificationDate = DateTime.UtcNow.ToUnspecified(),
            OldClientBalance = oldBal,
            NewClientBalance = newBal
        };

    private async Task<(FinanceTransaction Transaction, TransactionHistory RevertedHistory)>
        GetTransactionWithHistoryAsync(Guid id, CancellationToken token)
    {
        var tx = await _dbContext.FinanceTransaction
            .Include(t => t.TransactionHistories)
            .FirstOrDefaultAsync(t => t.Id == id, token);

        if (tx == null) throw new TransactionNotFoundException(id);

        var reverted = tx.TransactionHistories.FirstOrDefault(h => h.Status == TransactionStatus.Reverted);
        return (tx, reverted);
    }
}