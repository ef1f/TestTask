namespace TestTask.Core.Entities;

/// <summary>
/// Транзакция
/// </summary>
public class Transaction
{
    /// <summary>
    /// Id Транзакции
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Id Клиента
    /// </summary>
    public Guid ClientId { get; set; }

    /// <summary>
    /// Дата и время транзакции
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    /// Сумма операции 
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Тип транзакции
    /// </summary>
    public TransactionType TransactionType { get; set; }

    /// <summary>
    /// Статус транзакции принята/отклонена
    /// </summary>
    public TransactionStatus Status { get; set; }

    /// <summary>
    /// Дата выполнения транзакции
    /// </summary>
    public DateTime InsertedAt { get; set; }

    /// <summary>
    /// Дата отмены транзакции (если была отмена)
    /// </summary>
    public DateTime? RevertedAt { get; set; }

    /// <summary>
    /// Баланс клиента после проведения операции
    /// </summary>
    public decimal ClientBalance { get; set; }

    /// <summary>
    /// Клиент
    /// </summary>
    public Client Client { get; set; }
}