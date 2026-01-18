using Microsoft.EntityFrameworkCore;
using Npgsql;
using TestTask.Core;
using TestTask.Core.Entities;
using TestTask.Core.Exceptions;
using TestTask.Infrastructure.Data;

namespace TestTask.Infrastructure.Repositories;

public class ClientSpecificRepository : IClientSpecificRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ClientSpecificRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Client> GetClientForUpdateAsync(Guid clientId, CancellationToken token)
    {
        var client = await _dbContext.Clients
            .FromSqlRaw("SELECT id, name, balance FROM clients WHERE id = @id FOR UPDATE",
                new NpgsqlParameter("@id", clientId))
            .FirstOrDefaultAsync(token);

        return client ?? throw new ClientNotFoundException(clientId);
    }
}