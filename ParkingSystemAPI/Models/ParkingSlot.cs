using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingSystemAPI.Models
{
    public class ParkingSlot
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string SlotNumber { get; set; }

        [Required]
        [StringLength(20)]
        public string Zone { get; set; } // A, B, C, VIP, etc.

        [Required]
        [StringLength(20)]
        public string SlotType { get; set; } = "Regular"; // Regular, Disabled, Electric, VIP

        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<Fine> Fines { get; set; } = new List<Fine>();
    }
}
