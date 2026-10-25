
namespace DeveloperProjectTracker.API.DTOs.Tasks
{
    public class AssignTaskRequestDto
    {
        public List<Guid> AssignedToIds { get; set; } = new();
    }
}
