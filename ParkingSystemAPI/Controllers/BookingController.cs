using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParkingSystemAPI.DTOs;
using ParkingSystemAPI.Hubs;
using ParkingSystemAPI.Services.Interfaces;
using System.Security.Claims;

namespace ParkingSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly INotificationHubService _notificationService;

        public BookingController(IBookingService bookingService, INotificationHubService notificationService)
        {
            _bookingService = bookingService;
            _notificationService = notificationService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse("5");
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin") || User.IsInRole("Security");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetBookings([FromQuery] string status = null)
        {
            try
            {
                IEnumerable<BookingDto> bookings;

                if (IsAdmin())
                {
                    bookings = await _bookingService.GetAllBookingsAsync(status);
            }
                else
            {
                var userId = GetCurrentUserId();
                bookings = await _bookingService.GetUserBookingsAsync(userId, status);
            }

            return Ok(bookings);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookingDto>> GetBooking(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var booking = await _bookingService.GetBookingByIdAsync(id, userId);

                if (booking == null && !IsAdmin())
                {
                    return NotFound();
                }

                // If admin, allow access to any booking
                if (booking == null && IsAdmin())
                {
                    // For admin, we need a different method that doesn't filter by userId
                    return NotFound();
                }

                return Ok(booking);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("check-availability")]
        public async Task<ActionResult<BookingAvailabilityDto>> CheckAvailability([FromBody] CheckAvailabilityDto checkDto)
        {
            try
            {
                var availability = await _bookingService.CheckAvailabilityAsync(
                    checkDto.ParkingSlotId,
                    checkDto.StartTime,
                    checkDto.DurationHours
                );

                return Ok(availability);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<BookingDto>> CreateBooking([FromBody] CreateBookingDto createBookingDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var booking = await _bookingService.CreateBookingAsync(createBookingDto, userId);

                // Send real-time notification
                await _notificationService.SendNotificationToUserAsync(userId, new
                {
                    Type = "BookingCreated",
                    Title = "Booking Created",
                    Message = $"Your booking for slot {booking.ParkingSlot.SlotNumber} has been created successfully.",
                    BookingId = booking.Id,
                    Timestamp = DateTime.UtcNow
                });

                return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, booking);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/check-extension")]
        public async Task<ActionResult<BookingExtensionDto>> CheckExtension(int id, [FromBody] ExtendBookingDto extendDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var extensionCheck = await _bookingService.CheckExtensionPossibilityAsync(id, extendDto.AdditionalHours, userId);

                return Ok(extensionCheck);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/extend")]
        public async Task<ActionResult<BookingDto>> ExtendBooking(int id, [FromBody] ExtendBookingDto extendDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var extendedBooking = await _bookingService.ExtendBookingAsync(id, extendDto, userId);

                // Send real-time notification
                await _notificationService.SendNotificationToUserAsync(userId, new
                {
                    Type = "BookingExtended",
                    Title = "Booking Extended",
                    Message = $"Your booking has been extended by {extendDto.AdditionalHours} hour(s). Additional payment required.",
                    BookingId = id,
                    AdditionalHours = extendDto.AdditionalHours,
                    Timestamp = DateTime.UtcNow
                });

                return Ok(extendedBooking);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _bookingService.CancelBookingAsync(id, userId);

                if (!success)
                {
                    return NotFound(new { message = "Booking not found or cannot be cancelled" });
                }

                // Send real-time notification
                await _notificationService.SendNotificationToUserAsync(userId, new
                {
                    Type = "BookingCancelled",
                    Title = "Booking Cancelled",
                    Message = "Your booking has been cancelled successfully.",
                    BookingId = id,
                    Timestamp = DateTime.UtcNow
                });

                return Ok(new { message = "Booking cancelled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/complete")]
        [Authorize(Roles = "Admin,Security")]
        public async Task<IActionResult> CompleteBooking(int id)
        {
            try
            {
                var success = await _bookingService.CompleteBookingAsync(id);

                if (!success)
                {
                    return NotFound(new { message = "Booking not found or cannot be completed" });
                }

                return Ok(new { message = "Booking completed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("expiring")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<BookingDto>>> GetExpiringBookings([FromQuery] int minutes = 15)
        {
            try
            {
                var expiringBookings = await _bookingService.GetExpiringBookingsAsync(minutes);
                return Ok(expiringBookings);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    // Additional DTO for check availability endpoint
    public class CheckAvailabilityDto
    {
        public int ParkingSlotId { get; set; }
        public DateTime StartTime { get; set; }
        public int DurationHours { get; set; }
    }
}