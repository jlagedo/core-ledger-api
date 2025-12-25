using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreLedger.Domain.Entities;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for ToDo entity.
/// </summary>
public class ToDoConfiguration : IEntityTypeConfiguration<ToDo>
{
    public void Configure(EntityTypeBuilder<ToDo> builder)
    {
        builder.ToTable("todos");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.IsCompleted)
            .HasColumnName("is_completed")
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(t => t.CompletedAt)
            .HasColumnName("completed_at");

        builder.HasIndex(t => t.IsCompleted)
            .HasDatabaseName("ix_todos_is_completed");
    }
}
