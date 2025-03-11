using Hotel_Booking_App.Contexts;
using Hotel_Booking_App.Models;
using Microsoft.EntityFrameworkCore;

public class BookingCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingCleanupService> _logger;

    public BookingCleanupService(IServiceScopeFactory scopeFactory, ILogger<BookingCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<HotelBookingDbContext>();
                await ReleaseExpiredBookingsAsync(context);
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken); 
        }
    }

    private async Task ReleaseExpiredBookingsAsync(HotelBookingDbContext _context)
    {
        _logger.LogInformation("Running expired booking cleanup...");

        var expiredBookings = await _context.Bookings
            .Where(b => b.Status == BookingStatus.Paid && b.CheckOutDate < DateTime.UtcNow)
            .ToListAsync();

        foreach (var booking in expiredBookings)
        {
            var rooms = await _context.BookingRooms
                .Where(br => br.BookingId == booking.Id)
                .ToListAsync();

            _context.BookingRooms.RemoveRange(rooms);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Expired bookings released successfully.");
    }
}
