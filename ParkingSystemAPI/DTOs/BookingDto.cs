namespace ParkingSystemAPI.DTOs
{
    public class BookingDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ParkingSlotId { get; set; }
        public string LicensePlate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationHours { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserDto User { get; set; }
        public ParkingSlotDto ParkingSlot { get; set; }
    }

}
