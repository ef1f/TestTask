using TestTask.Application.Interfaces;
using TestTask.Core.Entities;

namespace TestTask.Application.Strategies;

public class DebitStrategy: ITransactionStrategy
{
    public TransactionType Type => TransactionType.Debit;
    public void Apply(Client client, decimal amount)
    {
        if (client.Balance < amount)
            throw new InvalidOperationException("Недостаточно средств");
        client.Balance -= amount;
    }
    public void Revert(Client client, decimal amount) => client.Balance += amount;
}