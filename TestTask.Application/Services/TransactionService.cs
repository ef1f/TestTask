using TestTask.Application.Interfaces;
using TestTask.Core;
using TestTask.Core.Models;

namespace TestTask.Application.Services;

public class TransactionService : ITransactionService
{
    public Task<TransactionResponse> CreditAsync(ITransaction transaction, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<TransactionResponse> DebitAsync(ITransaction transaction, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<RevertResponse> RevertAsync(Guid transactionId, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<BalanceResponse> GetBalanceAsync(Guid clientId, CancellationToken token)
    {
        throw new NotImplementedException();
    }
}