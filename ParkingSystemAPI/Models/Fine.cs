using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingSystemAPI.Models
{
    public class Fine
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("IssuedByUser")]
        public int IssuedByUserId { get; set; }

        [ForeignKey("User")]
        public int? UserId { get; set; } // Nullable if car owner is not registered

        [ForeignKey("ParkingSlot")]
        public int ParkingSlotId { get; set; }

        [ForeignKey("Booking")]
        public int? BookingId { get; set; } // Nullable if no booking exists

        [Required]
        [StringLength(20)]
        public string LicensePlate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(100)]
        public string Reason { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Issued"; // Issued, Paid, Disputed, Cancelled

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }

        // Navigation properties
        public virtual User IssuedByUser { get; set; }
        public virtual User User { get; set; }
        public virtual ParkingSlot ParkingSlot { get; set; }
        public virtual Booking Booking { get; set; }
    }
}
