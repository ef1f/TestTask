using TestTask.Core;
using TestTask.Core.Models;

namespace TestTask.Application.Interfaces;

/// <summary>
/// Сервис выполнения транзакций
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Операция начисления
    /// </summary>
    Task<TransactionResponse> CreditAsync(ITransaction transaction, CancellationToken token);

    /// <summary>
    /// Операция списания 
    /// </summary>
    Task<TransactionResponse> DebitAsync(ITransaction transaction, CancellationToken token);

    /// <summary>
    /// Отмена транзакции
    /// </summary>
    Task<RevertResponse> RevertAsync(Guid transactionId, CancellationToken token);

    /// <summary>
    /// Получение текущего баланса
    /// </summary>
    Task<BalanceResponse> GetBalanceAsync(Guid clientId, CancellationToken token);
}