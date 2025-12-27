using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreLedger.Domain.Entities;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Fund entity.
/// </summary>
public class FundConfiguration : IEntityTypeConfiguration<Fund>
{
    public void Configure(EntityTypeBuilder<Fund> builder)
    {
        builder.ToTable("funds");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(f => f.Code)
            .HasColumnName("code")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(f => f.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(f => f.BaseCurrency)
            .HasColumnName("base_currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(f => f.InceptionDate)
            .HasColumnName("inception_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(f => f.ValuationFrequency)
            .HasColumnName("valuation_frequency")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(f => f.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(f => f.Code)
            .IsUnique();

        builder.HasIndex(f => f.Name)
            .IsUnique();
    }
}
