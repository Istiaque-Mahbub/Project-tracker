using System;
using System.Collections.Generic;
using System.Text;

namespace DeveloperProjectTrackerDomain.Entities
{
    public class Label
    {
        public Guid Id { get; set; }

        public Guid ProjectId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Color { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties

        public Project Project { get; set; } = null!;

        public ICollection<TaskLabel> TaskLabels { get; set; }
            = new List<TaskLabel>();
    }
}
