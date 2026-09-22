using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeveloperProjectTracker.API.Data.Configurations
{
    public class MilestoneConfiguration : IEntityTypeConfiguration<Milestone>
    {
        public void Configure(EntityTypeBuilder<Milestone> builder)
        {
            builder.ToTable("Milestones");

            // Primary Key
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                .ValueGeneratedNever();

            // Project
            builder.Property(m => m.ProjectId)
                .IsRequired();

            // Name
            builder.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(200);

            // Description
            builder.Property(m => m.Description)
                .HasMaxLength(2000);

            // Dates
            builder.Property(m => m.StartDate)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(m => m.DueDate)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(m => m.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            // Project relationship
            builder.HasOne(m => m.Project)
                .WithMany(p => p.Milestones)
                .HasForeignKey(m => m.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Useful indexes
            builder.HasIndex(m => m.ProjectId);

            builder.HasIndex(m => m.DueDate);
        }
    }
}
