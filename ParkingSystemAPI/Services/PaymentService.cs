using AutoMapper;
using ParkingSystemAPI.DTOs;
using ParkingSystemAPI.Hubs;
using ParkingSystemAPI.Models;
using ParkingSystemAPI.Repository;
using ParkingSystemAPI.Services.Interfaces;

namespace ParkingSystemAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationHubService _hubService;

        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper, INotificationHubService hubService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hubService = hubService;
        }

        public async Task<PaymentDto> GetPaymentByIdAsync(int id)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(id, p => p.Booking);
            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task<IEnumerable<PaymentDto>> GetBookingPaymentsAsync(int bookingId)
        {
            var payments = await _unitOfWork.Payments.FindAsync(p => p.BookingId == bookingId, p => p.Booking);
            return _mapper.Map<IEnumerable<PaymentDto>>(payments);
        }

        public async Task<PaymentConfirmationDto> ProcessPaymentAsync(CreatePaymentDto createPaymentDto)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(createPaymentDto.BookingId, b => b.User, b => b.ParkingSlot);
            if (booking == null)
            {
                return new PaymentConfirmationDto
                {
                    Success = false,
                    Message = "Booking not found"
                };
            }

            // Create payment record
            var payment = new Payment
            {
                BookingId = createPaymentDto.BookingId,
                PaymentMethod = createPaymentDto.PaymentMethod,
                Amount = booking.TotalAmount,
                Status = "Pending",
                PaymentDetails = createPaymentDto.PaymentDetails,
                TransactionId = Guid.NewGuid().ToString("N")[..16] // Simulate transaction ID
            };

            await _unitOfWork.Payments.AddAsync(payment);

            // Simulate payment processing
            await Task.Delay(1000); // Simulate processing time

            // For demo purposes, assume payment is successful
            payment.Status = "Completed";
            payment.PaymentDate = DateTime.UtcNow;

            // Update booking status
            booking.PaymentStatus = "Completed";
            booking.Status = booking.Status == "Extended" ? "Active" : "Active";
            booking.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Payments.Update(payment);
            _unitOfWork.Bookings.Update(booking);

            await _unitOfWork.SaveChangesAsync();

            // Send real-time notification
            await _hubService.SendPaymentConfirmationAsync(booking.UserId, new
            {
                PaymentId = payment.Id,
                BookingId = booking.Id,
                Amount = payment.Amount,
                SlotNumber = booking.ParkingSlot.SlotNumber,
                Message = "Payment completed successfully"
            });

            return new PaymentConfirmationDto
            {
                Success = true,
                Message = "Payment processed successfully",
                Payment = _mapper.Map<PaymentDto>(payment),
                Booking = _mapper.Map<BookingDto>(booking)
            };
        }

        public async Task<bool> ConfirmPaymentAsync(int paymentId, string transactionId)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null || payment.Status != "Pending")
                return false;

            payment.Status = "Completed";
            payment.TransactionId = transactionId;
            payment.PaymentDate = DateTime.UtcNow;

            _unitOfWork.Payments.Update(payment);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RefundPaymentAsync(int paymentId, string reason)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null || payment.Status != "Completed")
                return false;

            payment.Status = "Refunded";
            payment.PaymentDetails += $" | Refunded: {reason}";

            _unitOfWork.Payments.Update(payment);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
