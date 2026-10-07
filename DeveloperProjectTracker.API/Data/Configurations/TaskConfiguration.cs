using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeveloperProjectTracker.API.Data.Configurations
{
    public class TaskConfiguration : IEntityTypeConfiguration<TaskItem>
    {
        public void Configure(EntityTypeBuilder<TaskItem> builder)
        {
            builder.ToTable("Tasks");

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .ValueGeneratedNever();

            // Project
            builder.Property(t => t.ProjectId)
                .IsRequired();

            // Title
            builder.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

            // Description
            builder.Property(t => t.Description)
                .HasMaxLength(5000);

            // Assigned User
            builder.Property(t => t.AssignedToId)
                .IsRequired(false);

            // Created User
            builder.Property(t => t.CreatedById)
                .IsRequired();

            // Milestone
            builder.Property(t => t.MilestoneId)
                .IsRequired(false);

            // Priority
            builder.Property(t => t.Priority)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

            // Status
            builder.Property(t => t.Status)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

            // Due Date
            builder.Property(t => t.DueDate)
                .HasColumnType("timestamp with time zone");

            // Dates
            builder.Property(t => t.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(t => t.UpdatedAt)
                .HasColumnType("timestamp with time zone");

            // Project -> Tasks
            builder.HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Assigned User
            builder.HasOne(t => t.AssignedTo)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);

            // Created User
            builder.HasOne(t => t.CreatedBy)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Milestone
            builder.HasOne(t => t.Milestone)
                .WithMany(m => m.Tasks)
                .HasForeignKey(t => t.MilestoneId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(t => t.ProjectId);

            builder.HasIndex(t => t.AssignedToId);

            builder.HasIndex(t => t.Status);

            builder.HasIndex(t => t.Priority);

            builder.HasIndex(t => t.DueDate);
        }
    }
}