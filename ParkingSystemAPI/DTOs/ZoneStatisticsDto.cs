namespace ParkingSystemAPI.DTOs
{
    public class ZoneStatisticsDto
    {
        public string Zone { get; set; }
        public int TotalSlots { get; set; }
        public int OccupiedSlots { get; set; }
        public int AvailableSlots { get; set; }
        public decimal OccupancyRate { get; set; }
    }
}
