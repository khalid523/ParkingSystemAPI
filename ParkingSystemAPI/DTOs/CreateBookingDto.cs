using System.ComponentModel.DataAnnotations;

namespace ParkingSystemAPI.DTOs
{
    public class CreateBookingDto
    {
        [Required]
        public int ParkingSlotId { get; set; }

        [Required]
        [StringLength(20)]
        public string LicensePlate { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        [Range(1, 24)]
        public int DurationHours { get; set; }
    }
}
