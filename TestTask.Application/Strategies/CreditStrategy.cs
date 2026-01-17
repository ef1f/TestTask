using TestTask.Application.Interfaces;
using TestTask.Core.Entities;

namespace TestTask.Application.Strategies;

public class CreditStrategy: ITransactionStrategy
{
    public TransactionType Type => TransactionType.Credit;
    public void Apply(Client client, decimal amount) => client.Balance += amount;
    
    public void Revert(Client client, decimal amount)
    {
        if (client.Balance < amount)
            throw new InvalidOperationException("Недостаточно средств для отмены зачисления (Credit)");
        client.Balance -= amount;
    }
}