using TestTask.Domain.Models;

namespace TestTask.Domain.Contracts;

public interface ITransactionService
{
    Task<TransactionResponse> CreditAsync(ITransaction transaction, CancellationToken token);
    Task<TransactionResponse> DebitAsync(ITransaction transaction, CancellationToken token);
    Task<RevertResponse> RevertAsync(Guid transactionId, CancellationToken token);
    Task<BalanceResponse> GetBalanceAsync(Guid clientId, CancellationToken token);
}