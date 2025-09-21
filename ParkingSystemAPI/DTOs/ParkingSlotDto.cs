namespace ParkingSystemAPI.DTOs
{
    public class ParkingSlotDto
    {
        public int Id { get; set; }
        public string SlotNumber { get; set; }
        public string Zone { get; set; }
        public string SlotType { get; set; }
        public decimal HourlyRate { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
        public bool IsAvailable { get; set; }
        public BookingDto CurrentBooking { get; set; }
    }
}
