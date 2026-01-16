
using TestTask.Core;
using TestTask.Core.Models;

namespace TestTask.Application.Interfaces;

public interface ITransactionService
{
    Task<TransactionResponse> CreditAsync(ITransaction transaction, CancellationToken token);
    Task<TransactionResponse> DebitAsync(ITransaction transaction, CancellationToken token);
    Task<RevertResponse> RevertAsync(Guid transactionId, CancellationToken token);
    Task<BalanceResponse> GetBalanceAsync(Guid clientId, CancellationToken token);
}