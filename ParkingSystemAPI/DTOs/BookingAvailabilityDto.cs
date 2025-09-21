namespace ParkingSystemAPI.DTOs
{
    public class BookingAvailabilityDto
    {
        public bool IsAvailable { get; set; }
        public string Message { get; set; }
        public BookingDto ConflictingBooking { get; set; }
        public decimal EstimatedCost { get; set; }
    }
}
