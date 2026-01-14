namespace TestTask.Domain.Models;

/// <summary>
/// Результат выполнения запроса на получение актуального баланса клиента
/// </summary>
public record BalanceResponse : BaseResponse
{
    /// <summary>
    /// Время выполнения запроса
    /// </summary>
    public DateTime BalanceDateTime { get; set; }
}