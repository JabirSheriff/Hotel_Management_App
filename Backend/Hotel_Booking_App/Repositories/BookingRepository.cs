using Hotel_Booking_App.Contexts;
using Hotel_Booking_App.Models;
using HotelBookingApp.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelBookingApp.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly HotelBookingDbContext _context;

        public BookingRepository(HotelBookingDbContext context)
        {
            _context = context;
        }

        public async Task<Booking> AddBookingAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<Booking?> GetBookingByIdAsync(int bookingId)
        {
            return await _context.Bookings
                .Include(b => b.Hotel) 
                .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
                .FirstOrDefaultAsync(b => b.Id == bookingId);
        }

        public async Task<List<Booking>> GetBookingsByCustomerIdAsync(int customerId)
        {
            return await _context.Bookings
                .Include(b => b.Hotel) 
                .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
                .Where(b => b.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<List<Room>> GetAvailableRoomsAsync(int hotelId, int numberOfRooms, RoomType roomType, int numberOfGuests, DateTime checkIn, DateTime checkOut)
        {
            var bookedRoomIds = await _context.BookingRooms
                .Where(br => br.Booking.CheckInDate < checkOut && br.Booking.CheckOutDate > checkIn)
                .Select(br => br.RoomId)
                .ToListAsync();

            return await _context.Rooms
                .Where(r => r.HotelId == hotelId &&
                            r.Type == roomType &&
                            r.Capacity >= numberOfGuests &&
                            !bookedRoomIds.Contains(r.Id))
                .Take(numberOfRooms)
                .ToListAsync();
        }

        public async Task<bool> UpdateBookingAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteBookingAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingRooms)
                .FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null) return false;

            _context.BookingRooms.RemoveRange(booking.BookingRooms);
            _context.Bookings.Remove(booking);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Room>> GetRoomsByHotelAndTypeAsync(int hotelId, RoomType roomType)
        {
            return await _context.Rooms
                .Where(r => r.HotelId == hotelId && r.Type == roomType)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetBookingsByHotelIdsAsync(List<int> hotelIds)
        {
            return await _context.Bookings
                .Where(b => hotelIds.Contains(b.HotelId))
                .Include(b => b.Hotel) 
                .Include(b => b.Customer)
                .ThenInclude(c => c.User)
                .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
                .ToListAsync();
        }
    }
}