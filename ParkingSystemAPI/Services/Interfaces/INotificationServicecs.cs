using ParkingSystemAPI.DTOs;

namespace ParkingSystemAPI.Services.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId, bool unreadOnly = false);
        Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto createNotificationDto);
        Task<bool> MarkAsReadAsync(int notificationId, int userId);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task SendBookingExpiryNotificationAsync(int bookingId);
        Task SendPaymentConfirmationNotificationAsync(int paymentId);
        Task SendFineNotificationAsync(int fineId);
    }
}
