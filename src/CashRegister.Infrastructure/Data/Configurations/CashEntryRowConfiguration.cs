using CashRegister.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashRegister.Infrastructure.Data.Configurations;

public class CashEntryRowConfiguration : IEntityTypeConfiguration<CashEntryRow>
{
    public void Configure(EntityTypeBuilder<CashEntryRow> builder)
    {
        builder.ToTable("CashEntryRows");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.RowType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.IsOutflow)
            .IsRequired();

        builder.Property(r => r.SequenceOrder)
            .IsRequired();

        builder.Property(r => r.Amount1000)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.Amount500)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.Amount200)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.Amount100)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.Amount50)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.Amount20)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.Amount10)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.Amount5)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.Amount2)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.Amount1)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.CoinAmount)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(r => new { r.CashEntryId, r.SequenceOrder });
    }
}
