using TestTask.Core.Entities;

namespace TestTask.Application.Interfaces;

public interface ITransactionStrategy
{
    TransactionType Type { get; }
    void Apply(Client client, decimal amount);
    void Revert(Client client, decimal amount);
}