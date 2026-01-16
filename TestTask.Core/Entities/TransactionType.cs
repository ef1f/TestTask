namespace TestTask.Core.Entities;

/// <summary>
/// Тип транзакции зачисления/списания
/// </summary>
public enum TransactionType
{
    /// <summary>
    ///  Зачисление средств
    /// </summary>
    Credit,

    /// <summary>
    /// Списание средств
    /// </summary>
    Debit
}