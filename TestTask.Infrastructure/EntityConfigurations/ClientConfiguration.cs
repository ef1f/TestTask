using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestTask.Core.Entities;

namespace TestTask.Infrastructure.EntityConfigurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).HasMaxLength(255);
        builder.Property(b => b.Balance)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0)
            .IsRequired();

        builder.HasCheckConstraint("CK_Client_Balance_Positive", "balance >= 0");
    }
}