using System.ComponentModel.DataAnnotations;

namespace ParkingSystemAPI.DTOs
{
    public class CreatePaymentDto
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [StringLength(500)]
        public string PaymentDetails { get; set; }
    }
}
