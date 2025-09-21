using System.ComponentModel.DataAnnotations;

namespace ParkingSystemAPI.DTOs
{
    public class ExtendBookingDto
    {
        [Required]
        [Range(1, 24)]
        public int AdditionalHours { get; set; }
    }
}
