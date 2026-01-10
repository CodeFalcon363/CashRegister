using CashRegister.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashRegister.Infrastructure.Data.Configurations;

public class CashEntryConfiguration : IEntityTypeConfiguration<CashEntry>
{
    public void Configure(EntityTypeBuilder<CashEntry> builder)
    {
        builder.ToTable("CashEntries");

        builder.HasKey(ce => ce.Id);

        builder.Property(ce => ce.EntryDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(ce => ce.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(ce => ce.RejectionReason)
            .HasMaxLength(500);

        builder.HasIndex(ce => new { ce.BranchId, ce.EntryDate })
            .IsUnique();

        builder.HasOne(ce => ce.CreatedBy)
            .WithMany()
            .HasForeignKey(ce => ce.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ce => ce.AuthorizedBy)
            .WithMany()
            .HasForeignKey(ce => ce.AuthorizedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(ce => ce.Rows)
            .WithOne(r => r.CashEntry)
            .HasForeignKey(r => r.CashEntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
