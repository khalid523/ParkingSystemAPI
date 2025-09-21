namespace ParkingSystemAPI.DTOs
{
    public class BookingExtensionDto
    {
        public bool CanExtend { get; set; }
        public string Message { get; set; }
        public int AdditionalHours { get; set; }
        public decimal AdditionalCost { get; set; }
        public BookingDto ExistingBooking { get; set; }
    }
}
