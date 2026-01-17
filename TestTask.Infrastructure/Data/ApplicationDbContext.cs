using Microsoft.EntityFrameworkCore;
using TestTask.Core.Models;
using TestTask.Core.Entities;

namespace TestTask.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<FinanceTransaction> FinanceTransaction { get; set; }

    public DbSet<TransactionHistory> TransactionHistory { get; set; }

    public DbSet<Client> Clients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        base.OnModelCreating(modelBuilder);
    }
}