using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeveloperProjectTracker.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

        public DbSet<Project> Projects => Set<Project>();

        public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();

        public DbSet<TaskItem> TaskItems => Set<TaskItem>();

        public DbSet<Comment> Comments => Set<Comment>();

        public DbSet<Label> Labels => Set<Label>();

        public DbSet<TaskLabel> TaskLables => Set<TaskLabel>();

        public DbSet<Milestone> Milestones => Set<Milestone>();

        public DbSet<WorkLog> Worklogs => Set<WorkLog>();

        public DbSet<Notification> Notifications => Set<Notification>();

        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        public DbSet<Tenant> Tenants => Set<Tenant>();

        public DbSet<TenantMembership> TenantMemberships => Set<TenantMembership>();

        public DbSet<TaskAssignment> TaskAssignments { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all IEntityTypeConfiguration<T>
            // classes from the API assembly.
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(ApplicationDbContext).Assembly);

            modelBuilder.Entity<TaskAssignment>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.Task)
                    .WithMany(x => x.Assignments)
                    .HasForeignKey(x => x.TaskId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.TaskId, x.UserId })
                    .IsUnique();
            });
        }
    }
}