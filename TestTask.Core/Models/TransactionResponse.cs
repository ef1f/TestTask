namespace TestTask.Core.Models;

/// <summary>
///  Результат операции зачисления/списания средств у клиента
/// </summary>
public record TransactionResponse : BaseResponse
{
    /// <summary>
    /// Дата и время операции
    /// </summary>
    public DateTime InsertDateTime { get; set; }
}