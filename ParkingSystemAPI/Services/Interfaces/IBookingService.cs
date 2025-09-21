using ParkingSystemAPI.DTOs;

namespace ParkingSystemAPI.Services.Interfaces
{
    public interface IBookingService
    {
        Task<BookingDto> GetBookingByIdAsync(int id, int userId);
        Task<IEnumerable<BookingDto>> GetUserBookingsAsync(int userId, string status = null);
        Task<IEnumerable<BookingDto>> GetAllBookingsAsync(string status = null);
        Task<BookingAvailabilityDto> CheckAvailabilityAsync(int slotId, DateTime startTime, int durationHours);
        Task<BookingDto> CreateBookingAsync(CreateBookingDto createBookingDto, int userId);
        Task<BookingExtensionDto> CheckExtensionPossibilityAsync(int bookingId, int additionalHours, int userId);
        Task<BookingDto> ExtendBookingAsync(int bookingId, ExtendBookingDto extendDto, int userId);
        Task<bool> CancelBookingAsync(int bookingId, int userId);
        Task<bool> CompleteBookingAsync(int bookingId);
        Task<List<BookingDto>> GetExpiringBookingsAsync(int minutesBeforeExpiry);
        Task MarkNotificationSentAsync(int bookingId);
    }

}
