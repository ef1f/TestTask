namespace TestTask.Core.Entities;

/// <summary>
/// История изменений по транзакциям 
/// </summary>
public class TransactionHistory
{
    /// <summary>
    /// Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///  Id Транзакции
    /// </summary>
    public Guid FinanceTransactionId { get; set; }

    /// <summary>
    /// Статус транзакции принята/отклонена
    /// </summary>
    public TransactionStatus Status { get; set; }

    /// <summary>
    /// Дата проведения
    /// </summary>
    public DateTime ModificationDate { get; set; }

    /// <summary>
    /// Баланс клиента до проведения операции
    /// </summary>
    public decimal OldClientBalance { get; set; }

    /// <summary>
    /// Баланс клиента после проведения операции
    /// </summary>
    public decimal NewClientBalance { get; set; }

    /// <summary>
    /// Транзакция
    /// </summary>
    public FinanceTransaction FinanceTransaction { get; set; }
}