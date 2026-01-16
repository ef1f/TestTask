using System.Text.Json.Serialization;

namespace TestTask.Core.Models;

/// <summary>
/// Результат выполнения запроса на получение актуального баланса клиента
/// </summary>
public record BalanceResponse : BaseResponse
{
    /// <summary>
    /// Время выполнения запроса
    /// </summary>
    [JsonConverter(typeof(PreciseDateTimeConverter))]
    public DateTime BalanceDateTime { get; set; }
}