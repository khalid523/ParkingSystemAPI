using AutoMapper;
using ParkingSystemAPI.DTOs;
using ParkingSystemAPI.Models;
using ParkingSystemAPI.Repository;
using ParkingSystemAPI.Services.Interfaces;

namespace ParkingSystemAPI.Services
{
    public class ParkingSlotService : IParkingSlotService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ParkingSlotService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ParkingSlotDto>> GetAllSlotsAsync()
        {
            var slots = await _unitOfWork.ParkingSlots.GetAllAsync();
            var slotDtos = new List<ParkingSlotDto>();

            foreach (var slot in slots)
            {
                var slotDto = _mapper.Map<ParkingSlotDto>(slot);
                slotDto.IsAvailable = await IsSlotAvailableAsync(slot.Id, DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
                slotDtos.Add(slotDto);
            }

            return slotDtos;
        }

        public async Task<IEnumerable<ParkingSlotDto>> GetAvailableSlotsAsync(DateTime startTime, int durationHours)
        {
            var endTime = startTime.AddHours(durationHours);
            var allSlots = await _unitOfWork.ParkingSlots.FindAsync(s => s.IsActive);
            var availableSlots = new List<ParkingSlotDto>();

            foreach (var slot in allSlots)
            {
                if (await IsSlotAvailableAsync(slot.Id, startTime, endTime))
                {
                    var slotDto = _mapper.Map<ParkingSlotDto>(slot);
                    slotDto.IsAvailable = true;
                    availableSlots.Add(slotDto);
                }
            }

            return availableSlots;
        }

        public async Task<ParkingSlotDto> GetSlotByIdAsync(int id)
        {
            var slot = await _unitOfWork.ParkingSlots.GetByIdAsync(id);
            if (slot == null) return null;

            var slotDto = _mapper.Map<ParkingSlotDto>(slot);
            slotDto.IsAvailable = await IsSlotAvailableAsync(id, DateTime.UtcNow, DateTime.UtcNow.AddHours(1));

            // Get current booking if any
            var currentBooking = await _unitOfWork.Bookings.FirstOrDefaultAsync(
                b => b.ParkingSlotId == id &&
                     b.Status == "Active" &&
                     b.StartTime <= DateTime.UtcNow &&
                     b.EndTime > DateTime.UtcNow,
                b => b.User
            );

            if (currentBooking != null)
            {
                slotDto.CurrentBooking = _mapper.Map<BookingDto>(currentBooking);
            }

            return slotDto;
        }

        public async Task<ParkingSlotDto> CreateSlotAsync(CreateParkingSlotDto createSlotDto)
        {
            if (await _unitOfWork.ParkingSlots.AnyAsync(s => s.SlotNumber == createSlotDto.SlotNumber))
            {
                throw new InvalidOperationException("Slot number already exists");
            }

            var slot = _mapper.Map<ParkingSlot>(createSlotDto);
            await _unitOfWork.ParkingSlots.AddAsync(slot);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ParkingSlotDto>(slot);
        }

        public async Task<bool> UpdateSlotAsync(int id, CreateParkingSlotDto updateSlotDto)
        {
            var slot = await _unitOfWork.ParkingSlots.GetByIdAsync(id);
            if (slot == null) return false;

            // Check if slot number is being changed and if it already exists
            if (updateSlotDto.SlotNumber != slot.SlotNumber &&
                await _unitOfWork.ParkingSlots.AnyAsync(s => s.SlotNumber == updateSlotDto.SlotNumber && s.Id != id))
            {
                throw new InvalidOperationException("Slot number already exists");
            }

            _mapper.Map(updateSlotDto, slot);
            _unitOfWork.ParkingSlots.Update(slot);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteSlotAsync(int id)
        {
            var slot = await _unitOfWork.ParkingSlots.GetByIdAsync(id);
            if (slot == null) return false;

            // Check if slot has active bookings
            var hasActiveBookings = await _unitOfWork.Bookings.AnyAsync(
                b => b.ParkingSlotId == id && (b.Status == "Active" || b.Status == "Pending")
            );

            if (hasActiveBookings)
            {
                throw new InvalidOperationException("Cannot delete slot with active bookings");
            }

            _unitOfWork.ParkingSlots.Remove(slot);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IsSlotAvailableAsync(int slotId, DateTime startTime, DateTime endTime, int? excludeBookingId = null)
        {
            var conflictingBooking = await _unitOfWork.Bookings.FirstOrDefaultAsync(
                b => b.ParkingSlotId == slotId &&
                     (excludeBookingId == null || b.Id != excludeBookingId) &&
                     b.Status != "Cancelled" &&
                     b.Status != "Completed" &&
                     ((b.StartTime <= startTime && b.EndTime > startTime) ||
                      (b.StartTime < endTime && b.EndTime >= endTime) ||
                      (b.StartTime >= startTime && b.EndTime <= endTime))
            );

            return conflictingBooking == null;
        }
    }
}
