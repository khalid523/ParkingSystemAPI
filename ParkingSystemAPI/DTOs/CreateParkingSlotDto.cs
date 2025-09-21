using System.ComponentModel.DataAnnotations;

namespace ParkingSystemAPI.DTOs
{
    public class CreateParkingSlotDto
    {
        [Required]
        [StringLength(10)]
        public string SlotNumber { get; set; }

        [Required]
        [StringLength(20)]
        public string Zone { get; set; }

        [Required]
        [StringLength(20)]
        public string SlotType { get; set; }

        [Required]
        public decimal HourlyRate { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
    }
}
