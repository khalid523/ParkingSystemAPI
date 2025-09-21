using ParkingSystemAPI.DTOs;

namespace ParkingSystemAPI.Services.Interfaces
{
    public interface IParkingSlotService
    {
        Task<IEnumerable<ParkingSlotDto>> GetAllSlotsAsync();
        Task<IEnumerable<ParkingSlotDto>> GetAvailableSlotsAsync(DateTime startTime, int durationHours);
        Task<ParkingSlotDto> GetSlotByIdAsync(int id);
        Task<ParkingSlotDto> CreateSlotAsync(CreateParkingSlotDto createSlotDto);
        Task<bool> UpdateSlotAsync(int id, CreateParkingSlotDto updateSlotDto);
        Task<bool> DeleteSlotAsync(int id);
        Task<bool> IsSlotAvailableAsync(int slotId, DateTime startTime, DateTime endTime, int? excludeBookingId = null);
    }

}
