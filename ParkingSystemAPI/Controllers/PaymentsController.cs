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
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDto>> GetPayment(int id)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByIdAsync(id);
                if (payment == null)
                    return NotFound();

                return Ok(payment);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("booking/{bookingId}")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetBookingPayments(int bookingId)
        {
            try
            {
                var payments = await _paymentService.GetBookingPaymentsAsync(bookingId);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("process")]
        public async Task<ActionResult<PaymentConfirmationDto>> ProcessPayment([FromBody] CreatePaymentDto createPaymentDto)
        {
            try
            {
                var result = await _paymentService.ProcessPaymentAsync(createPaymentDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> ConfirmPayment(int id, [FromBody] ConfirmPaymentDto confirmDto)
        {
            try
            {
                var success = await _paymentService.ConfirmPaymentAsync(id, confirmDto.TransactionId);
                if (!success)
                    return NotFound(new { message = "Payment not found or already processed" });

                return Ok(new { message = "Payment confirmed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/refund")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RefundPayment(int id, [FromBody] RefundPaymentDto refundDto)
        {
            try
            {
                var success = await _paymentService.RefundPaymentAsync(id, refundDto.Reason);
                if (!success)
                    return NotFound(new { message = "Payment not found or cannot be refunded" });

                return Ok(new { message = "Payment refunded successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    // Additional DTOs for payment confirmation
    public class ConfirmPaymentDto
    {
        public string TransactionId { get; set; }
    }

    public class RefundPaymentDto
    {
        public string Reason { get; set; }
    }
}