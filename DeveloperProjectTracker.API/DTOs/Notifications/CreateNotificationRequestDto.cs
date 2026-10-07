namespace DeveloperProjectTracker.API.DTOs.Notifications
{
    public class CreateNotificationRequestDto
    {
        public Guid UserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }
}
