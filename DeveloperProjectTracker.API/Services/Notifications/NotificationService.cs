using DeveloperProjectTracker.API.Data;
using DeveloperProjectTracker.API.DTOs.Notifications;
using DeveloperProjectTracker.API.Hubs;
using DeveloperProjectTrackerDomain.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DeveloperProjectTracker.API.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            ApplicationDbContext context,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // =========================================================
        // CREATE NOTIFICATION
        // =========================================================

        public async Task<NotificationResponseDto> CreateAsync(
            Guid userId,
            CreateNotificationRequestDto request)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException(
                    "User ID is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new ArgumentException(
                    "Notification title is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                throw new ArgumentException(
                    "Notification message is required.");
            }

            var userExists = await _context.Users
                .AnyAsync(u => u.Id == userId);

            if (!userExists)
            {
                throw new KeyNotFoundException(
                    "User not found.");
            }

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = request.Title.Trim(),
                Message = request.Message.Trim(),
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            var response = MapToResponse(notification);

            await _hubContext.Clients
                .Group($"user-{userId}")
                .SendAsync(
                    "ReceiveNotification",
                    response);

            return response;
        }

        // =========================================================
        // GET MY NOTIFICATIONS
        // =========================================================

        public async Task<List<NotificationResponseDto>>
            GetMyNotificationsAsync(Guid userId)
        {
            var userExists = await _context.Users
                .AnyAsync(u => u.Id == userId);

            if (!userExists)
            {
                throw new KeyNotFoundException(
                    "User not found.");
            }

            return await _context.Notifications
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new NotificationResponseDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Title = x.Title,
                    Message = x.Message,
                    IsRead = x.IsRead,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        // =========================================================
        // GET SINGLE NOTIFICATION
        // =========================================================

        public async Task<NotificationResponseDto> GetByIdAsync(
            Guid userId,
            Guid notificationId)
        {
            var notification = await _context.Notifications
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == notificationId &&
                    x.UserId == userId);

            if (notification is null)
            {
                throw new KeyNotFoundException(
                    "Notification not found.");
            }

            return MapToResponse(notification);
        }

        // =========================================================
        // MARK AS READ
        // =========================================================

        public async System.Threading.Tasks.Task MarkAsReadAsync(
            Guid userId,
            Guid notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(x =>
                    x.Id == notificationId &&
                    x.UserId == userId);

            if (notification is null)
            {
                throw new KeyNotFoundException(
                    "Notification not found.");
            }

            notification.IsRead = true;

            await _context.SaveChangesAsync();
        }

        // =========================================================
        // MARK ALL AS READ
        // =========================================================

        public async System.Threading.Tasks.Task MarkAllAsReadAsync(
            Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        // =========================================================
        // DELETE NOTIFICATION
        // =========================================================

        public async System.Threading.Tasks.Task DeleteAsync(
            Guid userId,
            Guid notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(x =>
                    x.Id == notificationId &&
                    x.UserId == userId);

            if (notification is null)
            {
                throw new KeyNotFoundException(
                    "Notification not found.");
            }

            _context.Notifications.Remove(notification);

            await _context.SaveChangesAsync();
        }

        // =========================================================
        // MAP RESPONSE
        // =========================================================

        private static NotificationResponseDto MapToResponse(
            Notification notification)
        {
            return new NotificationResponseDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
        }
    }
}