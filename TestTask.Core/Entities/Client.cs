namespace TestTask.Core.Entities;

/// <summary>
/// Клиент
/// </summary>
public class Client
{
    /// <summary>
    /// Id Клиента
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Название клиента
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Баланс клиента
    /// </summary>
    public decimal Balance { get; set; }
    
    /// <summary>
    /// Транзакции
    /// </summary>
    public ICollection<FinanceTransaction> Transactions { get; set; } = new List<FinanceTransaction>();
}