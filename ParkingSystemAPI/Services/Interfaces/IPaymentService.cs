using ParkingSystemAPI.DTOs;

namespace ParkingSystemAPI.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentDto> GetPaymentByIdAsync(int id);
        Task<IEnumerable<PaymentDto>> GetBookingPaymentsAsync(int bookingId);
        Task<PaymentConfirmationDto> ProcessPaymentAsync(CreatePaymentDto createPaymentDto);
        Task<bool> ConfirmPaymentAsync(int paymentId, string transactionId);
        Task<bool> RefundPaymentAsync(int paymentId, string reason);
    }

}
