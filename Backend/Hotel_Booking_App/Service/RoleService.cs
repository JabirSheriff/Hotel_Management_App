using Hotel_Booking_App.Contexts;
using Hotel_Booking_App.Interface;
using Hotel_Booking_App.Models;
using Hotel_Booking_App.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace Hotel_Booking_App.Service
{
    public class RoleService : IRoleService
    {
        private readonly HotelBookingDbContext _context;

        public RoleService(HotelBookingDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UpdateUserRoleAsync(UpdateUserRoleDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
            if (user == null) return false; 

            if (user.Role != "Unassigned") return false; 

            user.Role = dto.Role;
            await _context.SaveChangesAsync();

            
            if (dto.Role == "HotelOwner")
            {
                var hotelOwner = new HotelOwner
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email, 
                    PhoneNumber = user.PhoneNumber
                };
                _context.HotelOwners.Add(hotelOwner);
            }
            else if (dto.Role == "Customer")
            {
                var customer = new Customer
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber
                };
                
                _context.Customers.Add(customer);
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
