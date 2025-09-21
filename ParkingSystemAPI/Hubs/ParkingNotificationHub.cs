using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ParkingSystemAPI.Hubs
{
    [Authorize]
    public class ParkingNotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");

                // Add to role-based groups
                var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
                if (!string.IsNullOrEmpty(userRole))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"Role_{userRole}");
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");

                var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
                if (!string.IsNullOrEmpty(userRole))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Role_{userRole}");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Client can join specific groups
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        // Send typing indicator for chat features (if needed)
        public async Task SendTyping(string groupName)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst("firstName")?.Value;

            await Clients.GroupExcept(groupName, Context.ConnectionId)
                .SendAsync("UserTyping", new { UserId = userId, UserName = userName });
        }
    }

    // Service for sending notifications through SignalR
    public interface INotificationHubService
    {
        Task SendNotificationToUserAsync(int userId, object notification);
        Task SendNotificationToRoleAsync(string role, object notification);
        Task SendNotificationToAllAsync(object notification);
        Task SendBookingExpiryWarningAsync(int userId, object bookingData);
        Task SendPaymentConfirmationAsync(int userId, object paymentData);
        Task SendFineNotificationAsync(int userId, object fineData);
        Task SendSystemMaintenanceNotificationAsync(object maintenanceData);
    }

    public class NotificationHubService : INotificationHubService
    {
        private readonly IHubContext<ParkingNotificationHub> _hubContext;

        public NotificationHubService(IHubContext<ParkingNotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationToUserAsync(int userId, object notification)
        {
            await _hubContext.Clients.Group($"User_{userId}")
                .SendAsync("ReceiveNotification", notification);
        }

        public async Task SendNotificationToRoleAsync(string role, object notification)
        {
            await _hubContext.Clients.Group($"Role_{role}")
                .SendAsync("ReceiveNotification", notification);
        }

        public async Task SendNotificationToAllAsync(object notification)
        {
            await _hubContext.Clients.All
                .SendAsync("ReceiveNotification", notification);
        }

        public async Task SendBookingExpiryWarningAsync(int userId, object bookingData)
        {
            await _hubContext.Clients.Group($"User_{userId}")
                .SendAsync("BookingExpiryWarning", bookingData);
        }

        public async Task SendPaymentConfirmationAsync(int userId, object paymentData)
        {
            await _hubContext.Clients.Group($"User_{userId}")
                .SendAsync("PaymentConfirmation", paymentData);
        }

        public async Task SendFineNotificationAsync(int userId, object fineData)
        {
            await _hubContext.Clients.Group($"User_{userId}")
                .SendAsync("FineNotification", fineData);
        }

        public async Task SendSystemMaintenanceNotificationAsync(object maintenanceData)
        {
            await _hubContext.Clients.All
                .SendAsync("SystemMaintenance", maintenanceData);
        }
    }
}