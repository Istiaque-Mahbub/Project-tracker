using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeveloperProjectTracker.API.Data.Configurations
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable("Comments");

            // Primary Key
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .ValueGeneratedNever();

            // Content
            builder.Property(c => c.Content)
                .IsRequired()
                .HasMaxLength(5000);

            // Created At
            builder.Property(c => c.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            // Updated At
            builder.Property(c => c.UpdatedAt)
                .HasColumnType("timestamp with time zone");

            // Task relationship
            builder.HasOne(c => c.Task)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // User relationship
            builder.HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(c => c.TaskId);

            builder.HasIndex(c => c.UserId);
        }
    }
}
