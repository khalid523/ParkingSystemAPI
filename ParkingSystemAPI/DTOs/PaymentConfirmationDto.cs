namespace ParkingSystemAPI.DTOs
{
    public class PaymentConfirmationDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public PaymentDto Payment { get; set; }
        public BookingDto Booking { get; set; }
    }
}
