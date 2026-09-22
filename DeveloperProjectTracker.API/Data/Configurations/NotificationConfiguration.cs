using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeveloperProjectTracker.API.Data.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            // Primary Key
            builder.HasKey(n => n.Id);

            builder.Property(n => n.Id)
                .ValueGeneratedNever();

            // Title
            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200);

            // Message
            builder.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(2000);

            // Is Read
            builder.Property(n => n.IsRead)
                .IsRequired()
                .HasDefaultValue(false);

            // Created At
            builder.Property(n => n.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            // User relationship
            builder.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Useful indexes
            builder.HasIndex(n => n.UserId);

            builder.HasIndex(n => new
            {
                n.UserId,
                n.IsRead
            });

            builder.HasIndex(n => n.CreatedAt);
        }
    }
}
