using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestTask.Core.Entities;

namespace TestTask.Infrastructure.EntityConfigurations;

public class TransactionHistoryConfigurations : IEntityTypeConfiguration<TransactionHistory>
{
    public void Configure(EntityTypeBuilder<TransactionHistory> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.FinanceTransactionId).IsRequired();
        builder.Property(b => b.Status).IsRequired();
        builder.Property(b => b.ModificationDate).HasColumnType("timestamp without time zone").IsRequired();

        builder.Property(b => b.OldClientBalance)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(b => b.NewClientBalance)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0)
            .IsRequired();

        builder.HasIndex(b => new { b.FinanceTransactionId, b.Status })
            .IsUnique()
            .HasDatabaseName("IX_TransactionHistory_FinanceTransactionId");


        builder.HasOne(t => t.FinanceTransaction)
            .WithMany(c => c.TransactionHistories)
            .HasForeignKey(p => p.FinanceTransactionId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasCheckConstraint("CK_TransactionHistory_ClientBalance_Positive", "old_client_balance >= 0");
        builder.HasCheckConstraint("CK_TransactionHistory_NewClientBalance_Positive", "new_client_balance >= 0");
    }
}