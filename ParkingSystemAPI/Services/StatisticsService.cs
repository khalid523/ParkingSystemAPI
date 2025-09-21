using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ParkingSystemAPI.DTOs;
using ParkingSystemAPI.Repository;
using ParkingSystemAPI.Services.Interfaces;

namespace ParkingSystemAPI.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StatisticsService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ParkingStatisticsDto> GetParkingStatisticsAsync()
        {
            var totalSlots = await _unitOfWork.ParkingSlots.CountAsync(s => s.IsActive);
            var today = DateTime.Today;
            var activeBookings = await _unitOfWork.Bookings.CountAsync(
                b => b.Status == "Active" && b.StartTime <= DateTime.UtcNow && b.EndTime > DateTime.UtcNow
            );
            var todayBookings = await _unitOfWork.Bookings.CountAsync(
                b => b.CreatedAt >= today && b.CreatedAt < today.AddDays(1)
            );

            var todayPayments = await _unitOfWork.Payments.FindAsync(
                p => p.Status == "Completed" && p.PaymentDate >= today && p.PaymentDate < today.AddDays(1)
            );
            var todayRevenue = todayPayments.Sum(p => p.Amount);

            var pendingPayments = await _unitOfWork.Bookings.CountAsync(
                b => b.PaymentStatus == "Pending"
            );

            // Zone statistics
            var zones = await _unitOfWork.ParkingSlots.Query()
                .Where(s => s.IsActive)
                .GroupBy(s => s.Zone)
                .Select(g => new { Zone = g.Key, Count = g.Count() })
                .ToListAsync();

            var zoneStats = new List<ZoneStatisticsDto>();
            foreach (var zone in zones)
            {
                var occupiedInZone = await _unitOfWork.Bookings.Query()
                    .Join(_unitOfWork.ParkingSlots.Query(),
                          b => b.ParkingSlotId,
                          s => s.Id,
                          (b, s) => new { Booking = b, Slot = s })
                    .CountAsync(bs => bs.Slot.Zone == zone.Zone &&
                                     bs.Booking.Status == "Active" &&
                                     bs.Booking.StartTime <= DateTime.UtcNow &&
                                     bs.Booking.EndTime > DateTime.UtcNow);

                zoneStats.Add(new ZoneStatisticsDto
                {
                    Zone = zone.Zone,
                    TotalSlots = zone.Count,
                    OccupiedSlots = occupiedInZone,
                    AvailableSlots = zone.Count - occupiedInZone,
                    OccupancyRate = zone.Count > 0 ? (decimal)occupiedInZone / zone.Count * 100 : 0
                });
            }

            return new ParkingStatisticsDto
            {
                TotalSlots = totalSlots,
                OccupiedSlots = activeBookings,
                AvailableSlots = totalSlots - activeBookings,
                OccupancyRate = totalSlots > 0 ? (decimal)activeBookings / totalSlots * 100 : 0,
                TotalBookingsToday = todayBookings,
                TotalRevenueToday = todayRevenue,
                ActiveBookings = activeBookings,
                PendingPayments = pendingPayments,
                ZoneStatistics = zoneStats
            };
        }

        public async Task<UserBookingStatisticsDto> GetUserStatisticsAsync(int userId)
        {
            var totalBookings = await _unitOfWork.Bookings.CountAsync(b => b.UserId == userId);
            var activeBookings = await _unitOfWork.Bookings.CountAsync(
                b => b.UserId == userId && b.Status == "Active"
            );
            var completedBookings = await _unitOfWork.Bookings.CountAsync(
                b => b.UserId == userId && b.Status == "Completed"
            );

            var userPayments = await _unitOfWork.Bookings.Query()
                .Where(b => b.UserId == userId && b.PaymentStatus == "Completed")
                .SumAsync(b => b.TotalAmount);

            var userFines = await _unitOfWork.Fines.FindAsync(f => f.UserId == userId);
            var totalFinesAmount = userFines.Sum(f => f.Amount);

            var recentBookings = await _unitOfWork.Bookings.FindAsync(
                b => b.UserId == userId,
                b => b.ParkingSlot
            );
            var recentBookingDtos = _mapper.Map<List<BookingDto>>(
                recentBookings.OrderByDescending(b => b.CreatedAt).Take(5)
            );

            return new UserBookingStatisticsDto
            {
                TotalBookings = totalBookings,
                ActiveBookings = activeBookings,
                CompletedBookings = completedBookings,
                TotalAmountSpent = userPayments,
                TotalFines = userFines.Count(),
                TotalFinesAmount = totalFinesAmount,
                RecentBookings = recentBookingDtos
            };
        }

        public async Task<object> GetRevenueStatisticsAsync(DateTime fromDate, DateTime toDate)
        {
            var payments = await _unitOfWork.Payments.FindAsync(
                p => p.Status == "Completed" &&
                     p.PaymentDate >= fromDate &&
                     p.PaymentDate <= toDate
            );

            var dailyRevenue = payments
                .GroupBy(p => p.PaymentDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(p => p.Amount),
                    TransactionCount = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            return new
            {
                TotalRevenue = payments.Sum(p => p.Amount),
                TotalTransactions = payments.Count(),
                AverageTransaction = payments.Any() ? payments.Average(p => p.Amount) : 0,
                DailyRevenue = dailyRevenue
            };
        }

        public async Task<object> GetOccupancyTrendsAsync(DateTime fromDate, DateTime toDate)
        {
            var bookings = await _unitOfWork.Bookings.FindAsync(
                b => b.CreatedAt >= fromDate && b.CreatedAt <= toDate,
                b => b.ParkingSlot
            );

            var dailyOccupancy = bookings
                .GroupBy(b => b.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    BookingCount = g.Count(),
                    UniqueSlots = g.Select(b => b.ParkingSlotId).Distinct().Count(),
                    TotalHours = g.Sum(b => b.DurationHours)
                })
                .OrderBy(x => x.Date)
                .ToList();

            return new
            {
                TotalBookings = bookings.Count(),
                AverageBookingsPerDay = dailyOccupancy.Any() ? dailyOccupancy.Average(d => d.BookingCount) : 0,
                PeakDay = dailyOccupancy.OrderByDescending(d => d.BookingCount).FirstOrDefault(),
                DailyOccupancy = dailyOccupancy
            };
        }
    }
}