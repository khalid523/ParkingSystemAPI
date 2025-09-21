using ParkingSystemAPI.Hubs;
using ParkingSystemAPI.Services.Interfaces;

namespace ParkingSystemAPI.Services
{
    public class BookingExpiryBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingExpiryBackgroundService> _logger;
        private readonly IConfiguration _configuration;

        public BookingExpiryBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<BookingExpiryBackgroundService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var intervalMinutes = _configuration.GetValue<int>("NotificationSettings:BackgroundTaskIntervalMinutes", 5);
            var interval = TimeSpan.FromMinutes(intervalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessExpiringBookings();
                    await ProcessOverdueBookings();

                    // Run cleanup once per day
                    if (DateTime.UtcNow.Hour == 2) // Run at 2 AM
                    {
                        await CleanupOldNotifications();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in BookingExpiryBackgroundService");
                }

                await Task.Delay(interval, stoppingToken);
            }
        }

        private async Task ProcessExpiringBookings()
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var hubService = scope.ServiceProvider.GetRequiredService<INotificationHubService>();

            var warningMinutes = _configuration.GetValue<int>("NotificationSettings:BookingExpiryWarningMinutes", 15);
            var expiringBookings = await bookingService.GetExpiringBookingsAsync(warningMinutes);

            foreach (var booking in expiringBookings)
            {
                var minutesRemaining = (int)(booking.EndTime - DateTime.UtcNow).TotalMinutes;

                // Send database notification
                await notificationService.CreateNotificationAsync(new DTOs.CreateNotificationDto
                {
                    UserId = booking.UserId,
                    Title = "Parking Expiry Warning",
                    Message = $"Your parking in slot {booking.ParkingSlot.SlotNumber} will expire in {minutesRemaining} minutes.",
                    Type = "BookingExpiry"
                });

                // Send real-time notification
                await hubService.SendBookingExpiryWarningAsync(booking.UserId, new
                {
                    BookingId = booking.Id,
                    SlotNumber = booking.ParkingSlot.SlotNumber,
                    MinutesRemaining = minutesRemaining,
                    EndTime = booking.EndTime,
                    CanExtend = true, // Could check for conflicts here
                    Message = $"Your parking will expire in {minutesRemaining} minutes"
                });

                // Mark notification as sent
                await bookingService.MarkNotificationSentAsync(booking.Id);

                _logger.LogInformation($"Sent expiry warning for booking {booking.Id} to user {booking.UserId}");
            }
        }

        private async Task ProcessOverdueBookings()
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<Repository.IUnitOfWork>();

            // Find bookings that are overdue (ended but still marked as Active)
            var overdueBookings = await unitOfWork.Bookings.FindAsync(
                b => b.Status == "Active" && b.EndTime < DateTime.UtcNow,
                b => b.User, b => b.ParkingSlot
            );

            foreach (var booking in overdueBookings)
            {
                // Update booking status
                booking.Status = "Completed";
                booking.UpdatedAt = DateTime.UtcNow;
                unitOfWork.Bookings.Update(booking);

                _logger.LogInformation($"Automatically completed overdue booking {booking.Id}");
            }

            if (overdueBookings.Any())
            {
                await unitOfWork.SaveChangesAsync();
            }
        }

        private async Task CleanupOldNotifications()
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<Repository.IUnitOfWork>();

            var cleanupDays = _configuration.GetValue<int>("NotificationSettings:CleanupOldNotificationsDays", 30);
            var cutoffDate = DateTime.UtcNow.AddDays(-cleanupDays);

            var oldNotifications = await unitOfWork.Notifications.FindAsync(
                n => n.CreatedAt < cutoffDate && n.IsRead
            );

            if (oldNotifications.Any())
            {
                unitOfWork.Notifications.RemoveRange(oldNotifications);
                await unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Cleaned up {oldNotifications.Count()} old notifications");
            }
        }
    }
}