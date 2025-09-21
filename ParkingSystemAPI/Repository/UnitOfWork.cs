using Microsoft.EntityFrameworkCore.Storage;
using ParkingSystemAPI.Data;
using ParkingSystemAPI.Models;

namespace ParkingSystemAPI.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ParkingDbContext _context;
        private IDbContextTransaction _transaction;

        private IGenericRepository<User> _users;
        private IGenericRepository<ParkingSlot> _parkingSlots;
        private IGenericRepository<Booking> _bookings;
        private IGenericRepository<Payment> _payments;
        private IGenericRepository<Fine> _fines;
        private IGenericRepository<Notification> _notifications;

        public UnitOfWork(ParkingDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<User> Users =>
            _users ??= new GenericRepository<User>(_context);

        public IGenericRepository<ParkingSlot> ParkingSlots =>
            _parkingSlots ??= new GenericRepository<ParkingSlot>(_context);

        public IGenericRepository<Booking> Bookings =>
            _bookings ??= new GenericRepository<Booking>(_context);

        public IGenericRepository<Payment> Payments =>
            _payments ??= new GenericRepository<Payment>(_context);

        public IGenericRepository<Fine> Fines =>
            _fines ??= new GenericRepository<Fine>(_context);

        public IGenericRepository<Notification> Notifications =>
            _notifications ??= new GenericRepository<Notification>(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
