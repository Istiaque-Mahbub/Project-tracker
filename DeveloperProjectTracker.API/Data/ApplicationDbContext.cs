
using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeveloperProjectTracker.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
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


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(ApplicationDbContext).Assembly);
        }
    }
}