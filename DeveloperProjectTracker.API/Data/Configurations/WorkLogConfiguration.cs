using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeveloperProjectTracker.API.Data.Configurations
{
    public class WorkLogConfiguration : IEntityTypeConfiguration<WorkLog>
    {
        public void Configure(EntityTypeBuilder<WorkLog> builder)
        {
            builder.ToTable("WorkLogs");

            // Primary Key
            builder.HasKey(w => w.Id);

            builder.Property(w => w.Id)
                .ValueGeneratedNever();

            // Started At
            builder.Property(w => w.StartedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            // Ended At
            builder.Property(w => w.EndedAt)
                .HasColumnType("timestamp with time zone");

            // Duration
            builder.Property(w => w.DurationMinutes)
                .IsRequired(false);

            // Description
            builder.Property(w => w.Description)
                .HasMaxLength(2000);

            // Task relationship
            builder.HasOne(w => w.Task)
                .WithMany(t => t.WorkLogs)
                .HasForeignKey(w => w.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // User relationship
            builder.HasOne(w => w.User)
                .WithMany(u => u.WorkLogs)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(w => w.TaskId);

            builder.HasIndex(w => w.UserId);

            builder.HasIndex(w => w.StartedAt);
        }

    }
}
