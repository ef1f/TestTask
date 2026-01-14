namespace TestTask.Domain.Contracts;

/// <summary>
/// Транзакция
/// </summary>
public interface ITransaction
{
    /// <summary>
    /// Id Транзакции
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Id Клиента
    /// </summary>
    Guid ClientId { get; }

    /// <summary>
    /// Дата и время транзакции
    /// </summary>
    DateTime DateTime { get; }

    /// <summary>
    /// Сумма операции
    /// </summary>
    decimal Amount { get; }
}