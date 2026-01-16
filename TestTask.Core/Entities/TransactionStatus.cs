namespace TestTask.Core.Entities;

/// <summary>
/// Статус транзакции
/// </summary>
public enum TransactionStatus
{
    /// <summary>
    /// Выполнена
    /// </summary>
    Completed,

    /// <summary>
    /// Отменена
    /// </summary>
    Reverted
}