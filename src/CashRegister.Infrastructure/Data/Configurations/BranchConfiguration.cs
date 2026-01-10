using CashRegister.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashRegister.Infrastructure.Data.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.BranchCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(b => b.BranchName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.IsActive)
            .IsRequired();

        builder.HasIndex(b => b.BranchCode)
            .IsUnique();

        builder.HasMany(b => b.Users)
            .WithOne(u => u.Branch)
            .HasForeignKey(u => u.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.CashEntries)
            .WithOne(ce => ce.Branch)
            .HasForeignKey(ce => ce.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
