using AutoMapper;
using ParkingSystemAPI.DTOs;
using ParkingSystemAPI.Models;
using ParkingSystemAPI.Repository;
using ParkingSystemAPI.Services.Interfaces;

namespace ParkingSystemAPI.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BookingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<BookingDto> GetBookingByIdAsync(int id, int userId)
        {
            var booking = await _unitOfWork.Bookings.FirstOrDefaultAsync(
                b => b.Id == id && b.UserId == userId,
                b => b.User, b => b.ParkingSlot
            );

            return _mapper.Map<BookingDto>(booking);
        }

        public async Task<IEnumerable<BookingDto>> GetUserBookingsAsync(int userId, string status = null)
        {
            var query = _unitOfWork.Bookings.Query(b => b.UserId == userId);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(b => b.Status == status);
            }

            var bookings = await _unitOfWork.Bookings.FindAsync(
                query.Where(b => true).Select(b => b).ToList().Any() ?
                b => query.Any(q => q.Id == b.Id) : b => b.UserId == userId,
                b => b.User, b => b.ParkingSlot
            );

            return _mapper.Map<IEnumerable<BookingDto>>(bookings.OrderByDescending(b => b.CreatedAt));
        }

        public async Task<IEnumerable<BookingDto>> GetAllBookingsAsync(string status = null)
        {
            IEnumerable<Booking> bookings;

            if (!string.IsNullOrEmpty(status))
            {
                bookings = await _unitOfWork.Bookings.FindAsync(
                    b => b.Status == status,
                    b => b.User, b => b.ParkingSlot
                );
            }
            else
            {
                bookings = await _unitOfWork.Bookings.GetAllAsync(
                    b => b.User, b => b.ParkingSlot
                );
            }

            return _mapper.Map<IEnumerable<BookingDto>>(bookings.OrderByDescending(b => b.CreatedAt));
        }

        public async Task<BookingAvailabilityDto> CheckAvailabilityAsync(int slotId, DateTime startTime, int durationHours)
        {
            var endTime = startTime.AddHours(durationHours);

            // Get the parking slot
            var slot = await _unitOfWork.ParkingSlots.GetByIdAsync(slotId);
            if (slot == null || !slot.IsActive)
            {
                return new BookingAvailabilityDto
                {
                    IsAvailable = false,
                    Message = "Parking slot not found or inactive",
                    EstimatedCost = 0
                };
            }

            // Check for conflicting bookings
            var conflictingBooking = await _unitOfWork.Bookings.FirstOrDefaultAsync(
                b => b.ParkingSlotId == slotId &&
                     b.Status != "Cancelled" &&
                     b.Status != "Completed" &&
                     ((b.StartTime <= startTime && b.EndTime > startTime) ||
                      (b.StartTime < endTime && b.EndTime >= endTime) ||
                      (b.StartTime >= startTime && b.EndTime <= endTime)),
                b => b.User, b => b.ParkingSlot
            );

            var estimatedCost = slot.HourlyRate * durationHours;

            if (conflictingBooking != null)
            {
                return new BookingAvailabilityDto
                {
                    IsAvailable = false,
                    Message = "Slot is already booked for the selected time period",
                    ConflictingBooking = _mapper.Map<BookingDto>(conflictingBooking),
                    EstimatedCost = estimatedCost
                };
            }

            return new BookingAvailabilityDto
            {
                IsAvailable = true,
                Message = "Slot is available",
                EstimatedCost = estimatedCost
            };
        }

        public async Task<BookingDto> CreateBookingAsync(CreateBookingDto createBookingDto, int userId)
        {
            var availability = await CheckAvailabilityAsync(
                createBookingDto.ParkingSlotId,
                createBookingDto.StartTime,
                createBookingDto.DurationHours
            );

            if (!availability.IsAvailable)
            {
                throw new InvalidOperationException(availability.Message);
            }

            var slot = await _unitOfWork.ParkingSlots.GetByIdAsync(createBookingDto.ParkingSlotId);
            var endTime = createBookingDto.StartTime.AddHours(createBookingDto.DurationHours);
            var totalAmount = slot.HourlyRate * createBookingDto.DurationHours;

            var booking = new Booking
            {
                UserId = userId,
                ParkingSlotId = createBookingDto.ParkingSlotId,
                LicensePlate = createBookingDto.LicensePlate.ToUpper(),
                StartTime = createBookingDto.StartTime,
                EndTime = endTime,
                DurationHours = createBookingDto.DurationHours,
                TotalAmount = totalAmount,
                Status = "Pending",
                PaymentStatus = "Pending"
            };

            await _unitOfWork.Bookings.AddAsync(booking);
            await _unitOfWork.SaveChangesAsync();

            // Get the created booking with related data
            var createdBooking = await _unitOfWork.Bookings.FirstOrDefaultAsync(
                b => b.Id == booking.Id,
                b => b.User, b => b.ParkingSlot
            );

            return _mapper.Map<BookingDto>(createdBooking);
        }

        public async Task<BookingExtensionDto> CheckExtensionPossibilityAsync(int bookingId, int additionalHours, int userId)
        {
            var booking = await _unitOfWork.Bookings.FirstOrDefaultAsync(
                b => b.Id == bookingId && b.UserId == userId,
                b => b.ParkingSlot
            );

            if (booking == null)
            {
                return new BookingExtensionDto
                {
                    CanExtend = false,
                    Message = "Booking not found"
                };
            }

            if (booking.Status == "Completed" || booking.Status == "Cancelled")
            {
                return new BookingExtensionDto
                {
                    CanExtend = false,
                    Message = "Cannot extend completed or cancelled booking"
                };
            }

            var newEndTime = booking.EndTime.AddHours(additionalHours);

            // Check for conflicts with other bookings
            var conflictingBooking = await _unitOfWork.Bookings.FirstOrDefaultAsync(
                b => b.ParkingSlotId == booking.ParkingSlotId &&
                     b.Id != bookingId &&
                     b.Status != "Cancelled" &&
                     b.Status != "Completed" &&
                     b.StartTime < newEndTime && b.EndTime > booking.EndTime
            );

            var additionalCost = booking.ParkingSlot.HourlyRate * additionalHours;

            if (conflictingBooking != null)
            {
                return new BookingExtensionDto
                {
                    CanExtend = false,
                    Message = "Cannot extend due to conflicting booking",
                    AdditionalHours = additionalHours,
                    AdditionalCost = additionalCost,
                    ExistingBooking = _mapper.Map<BookingDto>(booking)
                };
            }

            return new BookingExtensionDto
            {
                CanExtend = true,
                Message = "Booking can be extended",
                AdditionalHours = additionalHours,
                AdditionalCost = additionalCost,
                ExistingBooking = _mapper.Map<BookingDto>(booking)
            };
        }

        public async Task<BookingDto> ExtendBookingAsync(int bookingId, ExtendBookingDto extendDto, int userId)
        {
            var extensionCheck = await CheckExtensionPossibilityAsync(bookingId, extendDto.AdditionalHours, userId);

            if (!extensionCheck.CanExtend)
            {
                throw new InvalidOperationException(extensionCheck.Message);
            }

            var booking = await _unitOfWork.Bookings.FirstOrDefaultAsync(
                b => b.Id == bookingId && b.UserId == userId,
                b => b.ParkingSlot
            );

            booking.EndTime = booking.EndTime.AddHours(extendDto.AdditionalHours);
            booking.DurationHours += extendDto.AdditionalHours;
            booking.TotalAmount += extensionCheck.AdditionalCost;
            booking.Status = "Extended";
            booking.PaymentStatus = "Pending"; // Reset payment status for the additional amount
            booking.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Bookings.Update(booking);
            await _unitOfWork.SaveChangesAsync();

            // Get updated booking with related data
            var updatedBooking = await _unitOfWork.Bookings.FirstOrDefaultAsync(
                b => b.Id == booking.Id,
                b => b.User, b => b.ParkingSlot
            );

            return _mapper.Map<BookingDto>(updatedBooking);
        }

        public async Task<bool> CancelBookingAsync(int bookingId, int userId)
        {
            var booking = await _unitOfWork.Bookings.FirstOrDefaultAsync(
                b => b.Id == bookingId && b.UserId == userId
            );

            if (booking == null || booking.Status == "Completed")
            {
                return false;
            }

            booking.Status = "Cancelled";
            booking.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Bookings.Update(booking);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CompleteBookingAsync(int bookingId)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);

            if (booking == null || booking.Status == "Cancelled")
            {
                return false;
            }

            booking.Status = "Completed";
            booking.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Bookings.Update(booking);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<List<BookingDto>> GetExpiringBookingsAsync(int minutesBeforeExpiry)
        {
            var cutoffTime = DateTime.UtcNow.AddMinutes(minutesBeforeExpiry);

            var expiringBookings = await _unitOfWork.Bookings.FindAsync(
                b => b.Status == "Active" &&
                     b.EndTime <= cutoffTime &&
                     b.EndTime > DateTime.UtcNow &&
                     !b.NotificationSent,
                b => b.User, b => b.ParkingSlot
            );

            return _mapper.Map<List<BookingDto>>(expiringBookings);
        }

        public async Task MarkNotificationSentAsync(int bookingId)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
            if (booking != null)
            {
                booking.NotificationSent = true;
                _unitOfWork.Bookings.Update(booking);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}
