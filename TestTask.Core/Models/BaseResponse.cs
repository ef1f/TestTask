namespace TestTask.Core.Models;

/// <summary>
/// Базовый класс результата выполненной операции 
/// </summary>
public record BaseResponse
{
    /// <summary>
    /// Баланс клиента
    /// </summary>
    public decimal ClientBalance { get; set; }
}