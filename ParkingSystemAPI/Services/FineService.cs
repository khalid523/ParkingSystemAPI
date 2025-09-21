using AutoMapper;
using ParkingSystemAPI.DTOs;
using ParkingSystemAPI.Hubs;
using ParkingSystemAPI.Models;
using ParkingSystemAPI.Repository;
using ParkingSystemAPI.Services.Interfaces;

namespace ParkingSystemAPI.Services
{
    public class FineService : IFineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationHubService _hubService;

        public FineService(IUnitOfWork unitOfWork, IMapper mapper, INotificationHubService hubService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hubService = hubService;
        }

        public async Task<IEnumerable<FineDto>> GetAllFinesAsync()
        {
            var fines = await _unitOfWork.Fines.GetAllAsync(
                f => f.IssuedByUser, f => f.User, f => f.ParkingSlot, f => f.Booking
            );
            return _mapper.Map<IEnumerable<FineDto>>(fines);
        }

        public async Task<IEnumerable<FineDto>> GetUserFinesAsync(int userId)
        {
            var fines = await _unitOfWork.Fines.FindAsync(
                f => f.UserId == userId,
                f => f.IssuedByUser, f => f.ParkingSlot, f => f.Booking
            );
            return _mapper.Map<IEnumerable<FineDto>>(fines);
        }

        public async Task<FineDto> GetFineByIdAsync(int id)
        {
            var fine = await _unitOfWork.Fines.GetByIdAsync(id,
                f => f.IssuedByUser, f => f.User, f => f.ParkingSlot, f => f.Booking
            );
            return _mapper.Map<FineDto>(fine);
        }

        public async Task<FineDto> IssueFineAsync(CreateFineDto createFineDto, int issuedByUserId)
        {
            // Try to find user by license plate from existing bookings
            var userBooking = await _unitOfWork.Bookings.FirstOrDefaultAsync(
                b => b.LicensePlate == createFineDto.LicensePlate.ToUpper(),
                b => b.User
            );

            var fine = new Fine
            {
                IssuedByUserId = issuedByUserId,
                UserId = userBooking?.UserId,
                ParkingSlotId = createFineDto.ParkingSlotId,
                BookingId = createFineDto.BookingId,
                LicensePlate = createFineDto.LicensePlate.ToUpper(),
                Amount = createFineDto.Amount,
                Reason = createFineDto.Reason,
                Description = createFineDto.Description,
                Status = "Issued"
            };

            await _unitOfWork.Fines.AddAsync(fine);
            await _unitOfWork.SaveChangesAsync();

            // Send notification if user is found
            if (userBooking != null)
            {
                await _hubService.SendFineNotificationAsync(userBooking.UserId, new
                {
                    FineId = fine.Id,
                    Amount = fine.Amount,
                    Reason = fine.Reason,
                    LicensePlate = fine.LicensePlate,
                    Message = $"A fine of ${fine.Amount} has been issued for license plate {fine.LicensePlate}"
                });
            }

            var createdFine = await _unitOfWork.Fines.GetByIdAsync(fine.Id,
                f => f.IssuedByUser, f => f.User, f => f.ParkingSlot, f => f.Booking
            );

            return _mapper.Map<FineDto>(createdFine);
        }

        public async Task<bool> PayFineAsync(int fineId)
        {
            var fine = await _unitOfWork.Fines.GetByIdAsync(fineId);
            if (fine == null || fine.Status != "Issued")
                return false;

            fine.Status = "Paid";
            fine.PaidAt = DateTime.UtcNow;

            _unitOfWork.Fines.Update(fine);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DisputeFineAsync(int fineId, string reason)
        {
            var fine = await _unitOfWork.Fines.GetByIdAsync(fineId);
            if (fine == null || fine.Status != "Issued")
                return false;

            fine.Status = "Disputed";
            fine.Description += $" | Disputed: {reason}";

            _unitOfWork.Fines.Update(fine);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CancelFineAsync(int fineId, int userId)
        {
            var fine = await _unitOfWork.Fines.GetByIdAsync(fineId);
            if (fine == null || fine.Status == "Paid")
                return false;

            fine.Status = "Cancelled";
            fine.Description += $" | Cancelled by user {userId}";

            _unitOfWork.Fines.Update(fine);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
