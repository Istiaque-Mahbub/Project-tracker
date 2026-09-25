using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeveloperProjectTracker.API.Data.Configurations
{
    public class UserConfiguration: IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id).ValueGeneratedNever();

            builder.Property(u => u.FullName).
                IsRequired().
                HasMaxLength(150);

            builder.Property(u => u.Email).
                IsRequired().
                HasMaxLength(150);
            builder.HasIndex(u => u.Email).
                IsUnique();

            builder.Property(u => u.PasswordHash).
                IsRequired().
                HasMaxLength(500);

            builder.Property(u => u.Role).
                HasConversion<string>().
                IsRequired();

            builder.Property(u => u.CreatedAt).
                HasColumnType("timestamp with time zone").
                IsRequired();

            builder.Property(u => u.UpdatedAt)
            .HasColumnType("timestamp with time zone");

            

            // User -> Owned Projects
            builder.HasMany(u => u.OwnedProjects)
                .WithOne(p => p.Owner)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Project Memberships
            builder.HasMany(u => u.ProjectMemberships)
                .WithOne(pm => pm.User)
                .HasForeignKey(pm => pm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Assigned Tasks
            builder.HasMany(u => u.AssignedTasks)
                .WithOne(t => t.AssignedTo)
                .HasForeignKey(t => t.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);

            // User -> Created Tasks
            builder.HasMany(u => u.CreatedTasks)
                .WithOne(t => t.CreatedBy)
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Comments
            builder.HasMany(u => u.Comments)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Work Logs
            builder.HasMany(u => u.WorkLogs)
                .WithOne(w => w.User)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Notifications
            builder.HasMany(u => u.Notifications)
                .WithOne(n => n.User)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Activities
            builder.HasMany(u => u.Activities)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Tenant Memberships
            builder.HasMany(u => u.TenantMemberships)
                .WithOne(tm => tm.User)
                .HasForeignKey(tm => tm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
