using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeveloperProjectTracker.API.Data.Configurations
{
    public class LabelConfiguration : IEntityTypeConfiguration<Label>
    {
        public void Configure(EntityTypeBuilder<Label> builder)
        {
            builder.ToTable("Labels");

            // Primary Key
            builder.HasKey(l => l.Id);

            builder.Property(l => l.Id)
                .ValueGeneratedNever();

            // Project
            builder.Property(l => l.ProjectId)
                .IsRequired();

            // Name
            builder.Property(l => l.Name)
                .IsRequired()
                .HasMaxLength(50);

            // Color
            builder.Property(l => l.Color)
                .HasMaxLength(20);

            // Created At
            builder.Property(l => l.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            // Project relationship
            builder.HasOne(l => l.Project)
                .WithMany(p => p.Labels)
                .HasForeignKey(l => l.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Same label name cannot exist twice in a project
            builder.HasIndex(l => new
            {
                l.ProjectId,
                l.Name
            })
            .IsUnique();
        }
    }
}

