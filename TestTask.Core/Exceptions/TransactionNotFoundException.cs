namespace TestTask.Core.Exceptions;

public class TransactionNotFoundException : Exception
{
    public TransactionNotFoundException()
    {
    }

    public TransactionNotFoundException(Guid transactionId) : base($"Транзакция не найдена Id = {transactionId}")
    {
    }

    /// <inheritdoc />
    public TransactionNotFoundException(string message)
        : base(message)
    {
    }


    /// <inheritdoc />
    public TransactionNotFoundException(string message, Exception innerException)
        : base(message, (Exception)innerException)
    {
    }
}