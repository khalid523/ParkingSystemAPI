using Microsoft.EntityFrameworkCore;
using ParkingSystemAPI.Models;

namespace ParkingSystemAPI.Data
{
    public class ParkingDbContext : DbContext
    {
        public ParkingDbContext(DbContextOptions<ParkingDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<ParkingSlot> ParkingSlots { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Fine> Fines { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // Define static values for seeding
        private static readonly DateTime FixedSeedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private const string AdminPasswordHash = "$2a$11$r3X5J7q9S1t8V2wB6yZ8KuY7N2vC1dF3gH4jK5L6M7N8B9V0C1X2Y3Z4A5B6C7D8E"; // Hash for "admin123"
        private const string SecurityPasswordHash = "$2a$11$s4Y6K8r0T2u9W3xC7yA9LvZ8N3wD1eF4gH5jK6L7M8N9B0C1X2Y3Z4A5B6C7D8F"; // Hash for "security123"

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure ParkingSlot entity
            modelBuilder.Entity<ParkingSlot>(entity =>
            {
                entity.HasIndex(e => e.SlotNumber).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.HourlyRate).HasPrecision(18, 2);
            });

            // Configure Booking entity
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);

                entity.HasOne(d => d.User)
                      .WithMany(p => p.Bookings)
                      .HasForeignKey(d => d.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.ParkingSlot)
                      .WithMany(p => p.Bookings)
                      .HasForeignKey(d => d.ParkingSlotId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Payment entity
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.PaymentDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.Amount).HasPrecision(18, 2);

                entity.HasOne(d => d.Booking)
                      .WithMany(p => p.Payments)
                      .HasForeignKey(d => d.BookingId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Fine entity
            modelBuilder.Entity<Fine>(entity =>
            {
                entity.Property(e => e.IssuedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.Amount).HasPrecision(18, 2);

                entity.HasOne(d => d.IssuedByUser)
                      .WithMany(p => p.IssuedFines)
                      .HasForeignKey(d => d.IssuedByUserId)
                      .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict

                entity.HasOne(d => d.User)
                      .WithMany(p => p.ReceivedFines)
                      .HasForeignKey(d => d.UserId)
                      .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict

                entity.HasOne(d => d.ParkingSlot)
                      .WithMany(p => p.Fines)
                      .HasForeignKey(d => d.ParkingSlotId)
                      .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict

                entity.HasOne(d => d.Booking)
                      .WithMany(p => p.Fines)
                      .HasForeignKey(d => d.BookingId)
                      .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict
            });

            // Configure Notification entity
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(d => d.User)
                      .WithMany()
                      .HasForeignKey(d => d.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@parking.com",
                    PhoneNumber = "1234567890",
                    PasswordHash = AdminPasswordHash, // Static hash
                    Role = "Admin",
                    CreatedAt = FixedSeedDate, // Static date
                    UpdatedAt = FixedSeedDate  // Static date
                },
                new User
                {
                    Id = 2,
                    FirstName = "Security",
                    LastName = "Guard",
                    Email = "security@parking.com",
                    PhoneNumber = "1234567891",
                    PasswordHash = SecurityPasswordHash, // Static hash
                    Role = "Security",
                    CreatedAt = FixedSeedDate, // Static date
                    UpdatedAt = FixedSeedDate  // Static date
                }
            );

            // Seed Parking Slots
            var parkingSlots = new List<ParkingSlot>();
            var slotId = 1;

            // Zone A - Regular slots
            for (int i = 1; i <= 20; i++)
            {
                parkingSlots.Add(new ParkingSlot
                {
                    Id = slotId++,
                    SlotNumber = $"A{i:D2}",
                    Zone = "A",
                    SlotType = "Regular",
                    HourlyRate = 5.00m,
                    Description = $"Regular parking slot A{i:D2}",
                    CreatedAt = FixedSeedDate // Static date
                });
            }

            // Zone B - Regular slots
            for (int i = 1; i <= 15; i++)
            {
                parkingSlots.Add(new ParkingSlot
                {
                    Id = slotId++,
                    SlotNumber = $"B{i:D2}",
                    Zone = "B",
                    SlotType = "Regular",
                    HourlyRate = 4.50m,
                    Description = $"Regular parking slot B{i:D2}",
                    CreatedAt = FixedSeedDate // Static date
                });
            }

            // Zone VIP - Premium slots
            for (int i = 1; i <= 5; i++)
            {
                parkingSlots.Add(new ParkingSlot
                {
                    Id = slotId++,
                    SlotNumber = $"VIP{i:D2}",
                    Zone = "VIP",
                    SlotType = "VIP",
                    HourlyRate = 10.00m,
                    Description = $"VIP parking slot VIP{i:D2}",
                    CreatedAt = FixedSeedDate // Static date
                });
            }

            // Disabled slots
            for (int i = 1; i <= 3; i++)
            {
                parkingSlots.Add(new ParkingSlot
                {
                    Id = slotId++,
                    SlotNumber = $"D{i:D2}",
                    Zone = "D",
                    SlotType = "Disabled",
                    HourlyRate = 3.00m,
                    Description = $"Disabled parking slot D{i:D2}",
                    CreatedAt = FixedSeedDate // Static date
                });
            }

            // Electric vehicle slots
            for (int i = 1; i <= 4; i++)
            {
                parkingSlots.Add(new ParkingSlot
                {
                    Id = slotId++,
                    SlotNumber = $"E{i:D2}",
                    Zone = "E",
                    SlotType = "Electric",
                    HourlyRate = 6.00m,
                    Description = $"Electric vehicle parking slot E{i:D2}",
                    CreatedAt = FixedSeedDate // Static date
                });
            }

            modelBuilder.Entity<ParkingSlot>().HasData(parkingSlots);
        }
    }
}