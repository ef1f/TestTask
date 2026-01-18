using Microsoft.EntityFrameworkCore;
using TestTask.Core.Entities;

namespace TestTask.Infrastructure.Data;

public static class DataSeeder
{
    public static void SeedDatabase(ApplicationDbContext dbContext)
    {
        if (dbContext.Clients.Any())
        {
            return;
        }

        var client = new Client
        {
            Id = Guid.Parse("cfaa0d3f-7fea-4423-9f69-ebff826e2f89"),
            Name = "Клиент 1",
            Balance = 0m
        };

        dbContext.Clients.Add(client);
        dbContext.SaveChanges();
    }
}