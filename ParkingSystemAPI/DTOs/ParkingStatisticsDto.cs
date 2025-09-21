namespace ParkingSystemAPI.DTOs
{
    public class ParkingStatisticsDto
    {
        public int TotalSlots { get; set; }
        public int OccupiedSlots { get; set; }
        public int AvailableSlots { get; set; }
        public decimal OccupancyRate { get; set; }
        public int TotalBookingsToday { get; set; }
        public decimal TotalRevenueToday { get; set; }
        public int ActiveBookings { get; set; }
        public int PendingPayments { get; set; }
        public List<ZoneStatisticsDto> ZoneStatistics { get; set; }
    }
}
