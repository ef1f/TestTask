using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestTask.Core.Entities;

namespace TestTask.Infrastructure.EntityConfigurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.DateTime).IsRequired();
        builder.Property(b => b.Amount).IsRequired();
        builder.Property(b => b.TransactionType).IsRequired();
        builder.Property(b => b.Status).IsRequired();
        builder.Property(b => b.InsertedAt).IsRequired();
        builder.Property(b => b.ClientBalance)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0)
            .IsRequired();

        builder.HasIndex(b => b.ClientId)
            .HasDatabaseName("IX_Transactions_ClientId");

        builder.HasOne(t => t.Client)
            .WithMany(c => c.Transactions)
            .HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasCheckConstraint("CK_Transaction_ClientBalance_Positive", "client_balance >= 0");
    }
}