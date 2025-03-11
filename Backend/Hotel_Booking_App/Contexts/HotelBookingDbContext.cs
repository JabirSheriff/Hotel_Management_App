using Hotel_Booking_App.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using HotelBookingApp.Models;

namespace Hotel_Booking_App.Contexts
{
    public class HotelBookingDbContext : DbContext
    {
        public HotelBookingDbContext(DbContextOptions<HotelBookingDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; } 
        public DbSet<HotelOwner> HotelOwners { get; set; } 
        public DbSet<Hotel> Hotels { get; set; } 
        public DbSet<Room> Rooms { get; set; } 
        public DbSet<Booking> Bookings { get; set; } 
        public DbSet<BookingRoom> BookingRooms { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Payment> Payments { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasDefaultValue("Unassigned");

            modelBuilder.Entity<User>()
                .Property(u => u.PasswordHash)
                .HasColumnType("varbinary(max)");

            modelBuilder.Entity<User>()
                .Property(u => u.PasswordSalt)
                .HasColumnType("varbinary(max)");

            // Relationships
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.User)
                .WithOne(u => u.Customer)
                .HasForeignKey<Customer>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Customer_User");

            modelBuilder.Entity<HotelOwner>()
                .HasOne(h => h.User)
                .WithOne(u => u.HotelOwner)
                .HasForeignKey<HotelOwner>(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_HotelOwner_User");
            // Hotel Relationships**************************************************************************
            modelBuilder.Entity<Hotel>()
                .HasOne(h => h.HotelOwner)
                .WithMany(o => o.Hotels)
                .HasForeignKey(h => h.HotelOwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Hotel>()
                .HasMany(h => h.Bookings)
                .WithOne(r => r.Hotel)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Room>()
                .HasOne(r => r.Hotel)
                .WithMany(h => h.Rooms)
                .HasForeignKey(r => r.HotelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Customer)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Hotel)
                .WithMany(h => h.Bookings)
                .HasForeignKey(b => b.HotelId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Booking>()
                .HasMany(b => b.BookingRooms)
                .WithOne()
                .HasForeignKey(br => br.BookingId)
                .OnDelete(DeleteBehavior.Cascade);



            modelBuilder.Entity<BookingRoom>()
                .HasKey(br => new { br.BookingId, br.RoomId });

            modelBuilder.Entity<BookingRoom>()
                .HasOne(br => br.Booking)
                .WithMany(b => b.BookingRooms)
                .HasForeignKey(br => br.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookingRoom>()
                .HasOne(br => br.Room)
                .WithMany(r => r.BookingRooms)
                .HasForeignKey(br => br.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            // Review Relationships**************************************************************************
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Customer)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Hotel)
                .WithMany(h => h.Reviews)
                .HasForeignKey(r => r.HotelId)
                .OnDelete(DeleteBehavior.Cascade);


            // Payment Relationships**************************************************************************
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithOne(b => b.Payment)
                .HasForeignKey<Payment>(p => p.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Customer)
                .WithMany()
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Hash the admin password and seed the admin user
            var adminPassword = "Admin@123";
            var (passwordHash, passwordSalt) = CreatePasswordHash(adminPassword);

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FullName = "Admin",
                    Email = "admin@gmail.com",
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    Role = "Admin",
                    PhoneNumber = "7397388965",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );
        }

        // Helper method to hash passwords
        private (byte[] PasswordHash, byte[] PasswordSalt) CreatePasswordHash(string password)
        {
            using var hmac = new HMACSHA512();
            return (hmac.ComputeHash(Encoding.UTF8.GetBytes(password)), hmac.Key);
        }
    }
}
