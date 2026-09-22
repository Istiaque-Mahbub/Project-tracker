using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeveloperProjectTracker.API.Data.Configurations
{
    public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
    {
        public void Configure(EntityTypeBuilder<ActivityLog> builder)
        {
            builder.ToTable("ActivityLogs");

            // Primary Key
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .ValueGeneratedNever();

            // Activity Type
            builder.Property(a => a.ActivityType)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(100);

            // Entity Type
            builder.Property(a => a.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            // Entity Id
            builder.Property(a => a.EntityId)
                .IsRequired(false);

            // Description
            builder.Property(a => a.Description)
                .IsRequired()
                .HasMaxLength(2000);

            // Created At
            builder.Property(a => a.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            // Project relationship
            builder.HasOne(a => a.Project)
                .WithMany(p => p.Activities)
                .HasForeignKey(a => a.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // User relationship
            builder.HasOne(a => a.User)
                .WithMany(u => u.Activities)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(a => a.ProjectId);

            builder.HasIndex(a => a.UserId);

            builder.HasIndex(a => a.CreatedAt);

            builder.HasIndex(a => new
            {
                a.EntityType,
                a.EntityId
            });

        }
    }
}