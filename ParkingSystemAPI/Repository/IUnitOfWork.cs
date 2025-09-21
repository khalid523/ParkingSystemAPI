using ParkingSystemAPI.Models;

namespace ParkingSystemAPI.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User> Users { get; }
        IGenericRepository<ParkingSlot> ParkingSlots { get; }
        IGenericRepository<Booking> Bookings { get; }
        IGenericRepository<Payment> Payments { get; }
        IGenericRepository<Fine> Fines { get; }
        IGenericRepository<Notification> Notifications { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }

}
