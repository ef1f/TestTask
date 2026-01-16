using System.Runtime.Serialization;

namespace TestTask.Core.Exceptions;

public class ClientNotFoundException : Exception
{
    /// <inheritdoc />
    public ClientNotFoundException()
    {
    }

    public ClientNotFoundException(Guid clientId) : base($"Клиент не найден Id = {clientId}")
    {
    }

    /// <inheritdoc />
    public ClientNotFoundException(string message)
        : base(message)
    {
    }


    /// <inheritdoc />
    public ClientNotFoundException(string message, Exception innerException)
        : base(message, (Exception)innerException)
    {
    }
}