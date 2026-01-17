using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestTask.Core.Entities;

namespace TestTask.Infrastructure.EntityConfigurations;

public class FinanceTransactionConfiguration : IEntityTypeConfiguration<FinanceTransaction>
{
    public void Configure(EntityTypeBuilder<FinanceTransaction> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.ClientId).IsRequired();
        builder.Property(b => b.DateTime).HasColumnType("timestamp without time zone").IsRequired();
        builder.Property(b => b.Amount).IsRequired();
        builder.Property(b => b.TransactionType).IsRequired();
 
        builder.HasIndex(b => b.ClientId)
            .HasDatabaseName("IX_FinanceTransaction_ClientId");

        builder.HasOne(t => t.Client)
            .WithMany(c => c.FinanceTransaction)
            .HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
       
    }
}