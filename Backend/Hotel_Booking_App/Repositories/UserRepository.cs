using Hotel_Booking_App.Contexts;
using Hotel_Booking_App.Interface;
using Hotel_Booking_App.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;


namespace Hotel_Booking_App.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly HotelBookingDbContext _context;

        public UserRepository(HotelBookingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<HotelOwner> GetHotelOwnerByUserIdAsync(int userId)
        {
            return await _context.HotelOwners.FirstOrDefaultAsync(h => h.UserId == userId);
        }

        public async Task<Customer> GetCustomerByUserIdAsync(int userId)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
        }


        public async Task<User?> GetLastUserAsync()
        {
            return await _context.Users.OrderByDescending(u => u.Id).FirstOrDefaultAsync();
        }

        public async Task<Customer?> GetLastCustomerAsync()
        {
            return await _context.Customers.OrderByDescending(c => c.Id).FirstOrDefaultAsync();
        }


        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
