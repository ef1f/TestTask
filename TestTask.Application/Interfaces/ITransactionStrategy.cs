using TestTask.Core.Entities;

namespace TestTask.Application.Interfaces;

/// <summary>
/// Стратегия обработки финансовой операции.
/// </summary>
public interface ITransactionStrategy
{
    /// <summary>
    /// Тип транзакции
    /// </summary>
    TransactionType Type { get; }

    /// <summary>
    /// Выполняет основную бизнес-логику изменения баланса клиента
    /// </summary>
    void Apply(Client client, decimal amount);

    /// <summary>
    /// Выполняет отмену операции, (откат) баланса
    /// </summary>
    void Revert(Client client, decimal amount);
}