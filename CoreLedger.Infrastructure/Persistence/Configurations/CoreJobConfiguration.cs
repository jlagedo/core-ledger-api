using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreLedger.Domain.Entities;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for CoreJob entity.
/// </summary>
public class CoreJobConfiguration : IEntityTypeConfiguration<CoreJob>
{
    public void Configure(EntityTypeBuilder<CoreJob> builder)
    {
        builder.ToTable("core_jobs");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(j => j.ReferenceId)
            .HasColumnName("reference_id")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(j => j.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(j => j.JobDescription)
            .HasColumnName("job_description")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(j => j.CreationDate)
            .HasColumnName("creation_date")
            .IsRequired();

        builder.Property(j => j.RunningDate)
            .HasColumnName("running_date");

        builder.Property(j => j.FinishedDate)
            .HasColumnName("finished_date");

        builder.Property(j => j.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(j => j.UpdatedAt)
            .HasColumnName("updated_at");

        // Non-unique index for query performance
        builder.HasIndex(j => j.ReferenceId);

        // Index for filtering by status
        builder.HasIndex(j => j.Status);
    }
}
