using System.Text.Json.Serialization;

namespace TestTask.Core.Models;

/// <summary>
/// Результат отмены транзакции
/// </summary>
public record RevertResponse : BaseResponse
{
    /// <summary>
    /// Дата и время отмены транзакции
    /// </summary>
    [JsonConverter(typeof(PreciseDateTimeConverter))]
    public DateTime RevertDateTime { get; set; }
}