using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeveloperProjectTracker.API.Data.Configurations
{
    public class ActivityLogConfiguration
        : IEntityTypeConfiguration<ActivityLog>
    {
        public void Configure(
            EntityTypeBuilder<ActivityLog> builder)
        {
            builder.ToTable("ActivityLogs");

            // Primary Key
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            // Action
            builder.Property(x => x.Action)
                .IsRequired()
                .HasMaxLength(100);

            // Description
            builder.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(1000);

            // CreatedAt
            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            // Tenant relationship
            // Deleting a tenant deletes its activity logs.
            builder.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Project relationship
            // Keep activity logs when a project is deleted.
            // ProjectId becomes NULL.
            builder.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);

            // Task relationship
            // Keep activity logs when a task is deleted.
            // TaskId becomes NULL.
            builder.HasOne(x => x.Task)
                .WithMany()
                .HasForeignKey(x => x.TaskId)
                .OnDelete(DeleteBehavior.SetNull);

            // User relationship
            // Prevent deleting a user if activity logs reference them.
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(x => x.TenantId);

            builder.HasIndex(x => x.ProjectId);

            builder.HasIndex(x => x.TaskId);

            builder.HasIndex(x => x.UserId);

            builder.HasIndex(x => x.CreatedAt);
        }
    }
}