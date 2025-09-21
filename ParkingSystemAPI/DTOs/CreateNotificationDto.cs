using System.ComponentModel.DataAnnotations;

namespace ParkingSystemAPI.DTOs
{
    public class CreateNotificationDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(500)]
        public string Message { get; set; }

        [Required]
        [StringLength(20)]
        public string Type { get; set; }
    }
}
