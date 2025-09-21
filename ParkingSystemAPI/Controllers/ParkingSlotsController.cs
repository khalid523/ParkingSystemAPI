using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParkingSystemAPI.DTOs;
using ParkingSystemAPI.Services.Interfaces;

namespace ParkingSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ParkingSlotsController : ControllerBase
    {
        private readonly IParkingSlotService _parkingSlotService;

        public ParkingSlotsController(IParkingSlotService parkingSlotService)
        {
            _parkingSlotService = parkingSlotService;
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin") || User.IsInRole("Security");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ParkingSlotDto>>> GetAllSlots()
        {
            try
            {
                var slots = await _parkingSlotService.GetAllSlotsAsync();
                return Ok(slots);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("available")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ParkingSlotDto>>> GetAvailableSlots(
            [FromQuery] DateTime startTime,
            [FromQuery] int durationHours)
        {
            try
            {
                var slots = await _parkingSlotService.GetAvailableSlotsAsync(startTime, durationHours);
                return Ok(slots);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ParkingSlotDto>> GetSlot(int id)
        {
            try
            {
                var slot = await _parkingSlotService.GetSlotByIdAsync(id);
                if (slot == null)
                    return NotFound();

                return Ok(slot);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ParkingSlotDto>> CreateSlot([FromBody] CreateParkingSlotDto createSlotDto)
        {
            try
            {
                var slot = await _parkingSlotService.CreateSlotAsync(createSlotDto);
                return CreatedAtAction(nameof(GetSlot), new { id = slot.Id }, slot);
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

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSlot(int id, [FromBody] CreateParkingSlotDto updateSlotDto)
        {
            try
            {
                var success = await _parkingSlotService.UpdateSlotAsync(id, updateSlotDto);
                if (!success)
                    return NotFound();

                return Ok(new { message = "Slot updated successfully" });
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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSlot(int id)
        {
            try
            {
                var success = await _parkingSlotService.DeleteSlotAsync(id);
                if (!success)
                    return NotFound();

                return Ok(new { message = "Slot deleted successfully" });
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
    }
}
