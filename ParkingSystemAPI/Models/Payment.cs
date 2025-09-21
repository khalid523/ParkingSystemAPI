using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingSystemAPI.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Booking")]
        public int BookingId { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } // CreditCard, DebitCard, Cash, Online

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded

        [StringLength(100)]
        public string TransactionId { get; set; }

        [StringLength(500)]
        public string PaymentDetails { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Booking Booking { get; set; }
    }
}
