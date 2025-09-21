using System.ComponentModel.DataAnnotations;

namespace ParkingSystemAPI.DTOs
{
    public class CreateFineDto
    {
        [Required]
        public int ParkingSlotId { get; set; }

        public int? BookingId { get; set; }

        [Required]
        [StringLength(20)]
        public string LicensePlate { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(100)]
        public string Reason { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
    }

}
