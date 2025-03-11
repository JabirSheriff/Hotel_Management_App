using Hotel_Booking_App.Contexts;
using Hotel_Booking_App.Interface.Review;
using Hotel_Booking_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Booking_App.Service
{
    public class CustomerService : ICustomerService
    {
        private readonly HotelBookingDbContext _context;

        public CustomerService(HotelBookingDbContext context)
        {
            _context = context;
        }

        public async Task<Customer?> GetCustomerByUserIdAsync(int userId)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
        }
    }
}
