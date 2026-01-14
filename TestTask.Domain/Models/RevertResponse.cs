namespace TestTask.Domain.Models;

/// <summary>
/// Результат отмены транзакции
/// </summary>
public record RevertResponse : BaseResponse
{
    /// <summary>
    /// Дата и время отмены транзакции
    /// </summary>
    public DateTime RevertDateTime { get; set; }
}