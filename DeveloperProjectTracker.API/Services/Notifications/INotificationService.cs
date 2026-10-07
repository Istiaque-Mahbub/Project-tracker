using DeveloperProjectTracker.API.DTOs.Notifications;

namespace DeveloperProjectTracker.API.Services.Notifications
{
    public interface INotificationService
    {
        Task<NotificationResponseDto> CreateAsync(
           Guid userId,
           CreateNotificationRequestDto request);

        Task<List<NotificationResponseDto>> GetMyNotificationsAsync(
            Guid userId);

        Task<NotificationResponseDto> GetByIdAsync(
            Guid userId,
            Guid notificationId);

        System.Threading.Tasks.Task MarkAsReadAsync(
            Guid userId,
            Guid notificationId);

        System.Threading.Tasks.Task MarkAllAsReadAsync(
            Guid userId);

        System.Threading.Tasks.Task DeleteAsync(
            Guid userId,
            Guid notificationId);
    }
}
