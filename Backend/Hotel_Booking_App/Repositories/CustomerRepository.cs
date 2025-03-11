using Hotel_Booking_App.Contexts;
using Hotel_Booking_App.Interface.Customer;
using Hotel_Booking_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Booking_App.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly HotelBookingDbContext _context;

        public CustomerRepository(HotelBookingDbContext context)
        {
            _context = context;
        }

        public async Task<Customer?> GetCustomerByIdAsync(int customerId)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Id == customerId);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
