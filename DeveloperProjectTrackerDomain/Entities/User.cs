

using DeveloperProjectTrackerDomain.Enums;
using System.ComponentModel.DataAnnotations;


namespace DeveloperProjectTrackerDomain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }


        public ICollection<Project> OwnedProjects { get; set; } =
            new List<Project>();
        public ICollection<ProjectMember> ProjectMemberships { get; set; } = 
            new List<ProjectMember>();
        public ICollection<TaskItem> AssignedTasks { get; set; }
        = new List<TaskItem>();

        public ICollection<TaskItem> CreatedTasks { get; set; }
            = new List<TaskItem>();

        public ICollection<Comment> Comments { get; set; }
            = new List<Comment>();

        public ICollection<WorkLog> WorkLogs { get; set; }
            = new List<WorkLog>();

        public ICollection<Notification> Notifications { get; set; }
            = new List<Notification>();

        public ICollection<ActivityLog> Activities { get; set; }
            = new List<ActivityLog>();

      public ICollection<RefreshToken> RefreshTokens { get; set; }
    = new List<RefreshToken>();

      public ICollection<TenantMembership> TenantMemberships { get; set; }
    = new List<TenantMembership>();
    }

    }

