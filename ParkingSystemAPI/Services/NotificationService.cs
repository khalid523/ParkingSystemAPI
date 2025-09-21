using AutoMapper;
using ParkingSystemAPI.DTOs;
using ParkingSystemAPI.Models;
using ParkingSystemAPI.Repository;
using ParkingSystemAPI.Services.Interfaces;

namespace ParkingSystemAPI.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public NotificationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId, bool unreadOnly = false)
        {
            IEnumerable<Notification> notifications;

            if (unreadOnly)
            {
                notifications = await _unitOfWork.Notifications.FindAsync(
                    n => n.UserId == userId && !n.IsRead
                );
            }
            else
            {
                notifications = await _unitOfWork.Notifications.FindAsync(
                    n => n.UserId == userId
                );
            }

            return _mapper.Map<IEnumerable<NotificationDto>>(notifications.OrderByDescending(n => n.CreatedAt));
        }

        public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto createNotificationDto)
        {
            var notification = _mapper.Map<Notification>(createNotificationDto);

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<NotificationDto>(notification);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _unitOfWork.Notifications.FirstOrDefaultAsync(
                n => n.Id == notificationId && n.UserId == userId
            );

            if (notification == null) return false;

            notification.IsRead = true;
            _unitOfWork.Notifications.Update(notification);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            var unreadNotifications = await _unitOfWork.Notifications.FindAsync(
                n => n.UserId == userId && !n.IsRead
            );

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            _unitOfWork.Notifications.UpdateRange(unreadNotifications);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task SendBookingExpiryNotificationAsync(int bookingId)
        {
            // Implementation handled by background service
            await Task.CompletedTask;
        }

        public async Task SendPaymentConfirmationNotificationAsync(int paymentId)
        {
            // Implementation handled by payment service
            await Task.CompletedTask;
        }

        public async Task SendFineNotificationAsync(int fineId)
        {
            // Implementation handled by fine service
            await Task.CompletedTask;
        }
    }
}
