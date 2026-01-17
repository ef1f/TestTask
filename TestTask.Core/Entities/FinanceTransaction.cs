namespace TestTask.Core.Entities;

/// <summary>
/// Транзакция
/// </summary>
public class FinanceTransaction
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
    /// Клиент
    /// </summary>
    public Client Client { get; set; }

    /// <summary>
    /// История изменений по транзакции 
    /// </summary>
    public ICollection<TransactionHistory> TransactionHistories { get; set; } = new List<TransactionHistory>();
}