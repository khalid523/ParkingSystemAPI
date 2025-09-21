using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParkingSystemAPI.DTOs;
using ParkingSystemAPI.Services.Interfaces;
using System.Security.Claims;

namespace ParkingSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FinesController : ControllerBase
    {
        private readonly IFineService _fineService;

        public FinesController(IFineService fineService)
        {
            _fineService = fineService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim);
        }

        private bool IsSecurityStaff()
        {
            return User.IsInRole("Admin") || User.IsInRole("Security");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FineDto>>> GetFines()
        {
            try
            {
                IEnumerable<FineDto> fines;

                if (IsSecurityStaff())
                {
                    fines = await _fineService.GetAllFinesAsync();
                }
                else
                {
                    var userId = GetCurrentUserId();
                    fines = await _fineService.GetUserFinesAsync(userId);
                }

                return Ok(fines);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FineDto>> GetFine(int id)
        {
            try
            {
                var fine = await _fineService.GetFineByIdAsync(id);
                if (fine == null)
                    return NotFound();

                // Users can only see their own fines unless they're security staff
                if (!IsSecurityStaff() && fine.UserId != GetCurrentUserId())
                    return Forbid();

                return Ok(fine);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Security")]
        public async Task<ActionResult<FineDto>> IssueFine([FromBody] CreateFineDto createFineDto)
        {
            try
            {
                var issuedByUserId = GetCurrentUserId();
                var fine = await _fineService.IssueFineAsync(createFineDto, issuedByUserId);

                return CreatedAtAction(nameof(GetFine), new { id = fine.Id }, fine);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/pay")]
        public async Task<IActionResult> PayFine(int id)
        {
            try
            {
                var success = await _fineService.PayFineAsync(id);
                if (!success)
                    return NotFound(new { message = "Fine not found or cannot be paid" });

                return Ok(new { message = "Fine paid successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/dispute")]
        public async Task<IActionResult> DisputeFine(int id, [FromBody] DisputeFineDto disputeDto)
        {
            try
            {
                var success = await _fineService.DisputeFineAsync(id, disputeDto.Reason);
                if (!success)
                    return NotFound(new { message = "Fine not found or cannot be disputed" });

                return Ok(new { message = "Fine disputed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CancelFine(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _fineService.CancelFineAsync(id, userId);
                if (!success)
                    return NotFound(new { message = "Fine not found or cannot be cancelled" });

                return Ok(new { message = "Fine cancelled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class DisputeFineDto
    {
        public string Reason { get; set; }
    }
}