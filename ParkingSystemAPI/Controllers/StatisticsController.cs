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
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim);
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin") || User.IsInRole("Security");
        }

        [HttpGet("parking")]
        [Authorize(Roles = "Admin,Security")]
        public async Task<ActionResult<ParkingStatisticsDto>> GetParkingStatistics()
        {
            try
            {
                var statistics = await _statisticsService.GetParkingStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("user")]
        public async Task<ActionResult<UserBookingStatisticsDto>> GetUserStatistics()
        {
            try
            {
                var userId = GetCurrentUserId();
                var statistics = await _statisticsService.GetUserStatisticsAsync(userId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("revenue")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetRevenueStatistics(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            try
            {
                var statistics = await _statisticsService.GetRevenueStatisticsAsync(fromDate, toDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("occupancy")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetOccupancyTrends(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            try
            {
                var trends = await _statisticsService.GetOccupancyTrendsAsync(fromDate, toDate);
                return Ok(trends);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}