using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeveloperProjectTracker.API.Data.Configurations
{
    public class TaskLabelConfiguration : IEntityTypeConfiguration<TaskLabel>
    {
        public void Configure(EntityTypeBuilder<TaskLabel> builder)
        {
            builder.ToTable("TaskLabels");

            // Composite Primary Key
            builder.HasKey(tl => new
            {
                tl.TaskId,
                tl.LabelId
            });

            // Task relationship
            builder.HasOne(tl => tl.Task)
                .WithMany(t => t.TaskLabels)
                .HasForeignKey(tl => tl.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // Label relationship
            builder.HasOne(tl => tl.Label)
                .WithMany(l => l.TaskLabels)
                .HasForeignKey(tl => tl.LabelId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
