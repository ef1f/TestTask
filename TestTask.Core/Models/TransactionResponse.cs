using System.Text.Json.Serialization;

namespace TestTask.Core.Models;

/// <summary>
///  Результат операции зачисления/списания средств у клиента
/// </summary>
public record TransactionResponse : BaseResponse
{
    /// <summary>
    /// Дата и время операции
    /// </summary>
    [JsonConverter(typeof(PreciseDateTimeConverter))]
    public DateTime InsertDateTime { get; set; }
}