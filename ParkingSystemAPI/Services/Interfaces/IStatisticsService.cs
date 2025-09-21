using ParkingSystemAPI.DTOs;

namespace ParkingSystemAPI.Services.Interfaces
{
    public interface IStatisticsService
    {
        Task<ParkingStatisticsDto> GetParkingStatisticsAsync();
        Task<UserBookingStatisticsDto> GetUserStatisticsAsync(int userId);
        Task<object> GetRevenueStatisticsAsync(DateTime fromDate, DateTime toDate);
        Task<object> GetOccupancyTrendsAsync(DateTime fromDate, DateTime toDate);
    }
}
