

namespace DeveloperProjectTrackerDomain.Entities
{
    public class WorkLog
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid UserId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int? DurationMinutes { get; set; }
        public string? Description { get; set; }
        public TaskItem Task { get; set; } = null!;
        public User User { get; set; } = null!;

    }
}
