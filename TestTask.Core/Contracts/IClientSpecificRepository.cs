using TestTask.Core.Entities;

namespace TestTask.Core;

public interface IClientSpecificRepository
{
    Task<Client> GetClientForUpdateAsync(Guid clientId, CancellationToken token);
}