namespace ParkingSystemAPI.Services.Interfaces
{
    public interface IBackgroundTaskService
    {
        Task ProcessExpiringBookingsAsync();
        Task ProcessOverdueBookingsAsync();
        Task CleanupOldNotificationsAsync();
        Task GenerateDailyReportsAsync();
    }
}
