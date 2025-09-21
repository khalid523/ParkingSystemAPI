namespace ParkingSystemAPI.DTOs
{
    public class FineDto
    {
        public int Id { get; set; }
        public int IssuedByUserId { get; set; }
        public int? UserId { get; set; }
        public int ParkingSlotId { get; set; }
        public int? BookingId { get; set; }
        public string LicensePlate { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public UserDto IssuedByUser { get; set; }
        public UserDto User { get; set; }
        public ParkingSlotDto ParkingSlot { get; set; }
    }
}
