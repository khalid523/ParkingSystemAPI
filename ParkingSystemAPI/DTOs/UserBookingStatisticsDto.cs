namespace ParkingSystemAPI.DTOs
{
    public class UserBookingStatisticsDto
    {
        public int TotalBookings { get; set; }
        public int ActiveBookings { get; set; }
        public int CompletedBookings { get; set; }
        public decimal TotalAmountSpent { get; set; }
        public int TotalFines { get; set; }
        public decimal TotalFinesAmount { get; set; }
        public List<BookingDto> RecentBookings { get; set; }
    }
}
