using TestTask.Domain.Contracts;

namespace TestTask.Domain.Models;

public abstract record BaseTransaction : ITransaction
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; }
    public DateTime DateTime { get; init; }
    public decimal Amount { get; init; }
}