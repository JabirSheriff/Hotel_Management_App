using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hotel_Booking_App.Contexts;
using Hotel_Booking_App.Interface.Hotel_Room;
using Hotel_Booking_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Booking_App.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly HotelBookingDbContext _context;

        public RoomRepository(HotelBookingDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkInDate, DateTime checkOutDate)
        {
            var roomExists = await _context.Rooms.AnyAsync(r => r.Id == roomId);
            if (!roomExists) return false;

            return !await _context.BookingRooms
                .AnyAsync(br =>
                    br.RoomId == roomId &&
                    ((checkInDate >= br.CheckInDate && checkInDate < br.CheckOutDate) ||
                    (checkOutDate > br.CheckInDate && checkOutDate <= br.CheckOutDate) ||
                    (checkInDate <= br.CheckInDate && checkOutDate >= br.CheckOutDate)));

        }
        public async Task<bool> AddRoomAsync(Room room)
        {
            await _context.Rooms.AddAsync(room);
            return await _context.SaveChangesAsync() > 0;  
        }

        public async Task<Room?> GetRoomByIdAsync(int roomId)
        {
            return await _context.Rooms
                .Include(r => r.Hotel)
                .FirstOrDefaultAsync(r => r.Id == roomId);
        }

        public async Task<IEnumerable<Room>> GetRoomsByHotelIdAsync(int hotelId)
        {
            return await _context.Rooms
                .Where(r => r.HotelId == hotelId)
                .ToListAsync();
        }

        public async Task<List<Room>> GetAvailableRoomsAsync(int hotelId, int roomType, DateTime checkIn, DateTime checkOut)
        {
            var bookedRoomIds = await _context.BookingRooms
    .Where(br => br.Room.HotelId == hotelId &&
               ((checkIn >= br.CheckInDate && checkIn < br.CheckOutDate) ||
                (checkOut > br.CheckInDate && checkOut <= br.CheckOutDate) ||
                (checkIn <= br.CheckInDate && checkOut >= br.CheckOutDate)))
    .Select(br => br.RoomId)
    .Distinct()
    .ToListAsync();

            return await _context.Rooms
            .Where(r => r.HotelId == hotelId &&
                 (int)r.Type == roomType &&
                 !_context.BookingRooms
                     .Any(br => br.RoomId == r.Id &&
                                ((checkIn >= br.CheckInDate && checkIn < br.CheckOutDate) ||
                                 (checkOut > br.CheckInDate && checkOut <= br.CheckOutDate) ||
                                 (checkIn <= br.CheckInDate && checkOut >= br.CheckOutDate))))
                        .ToListAsync();
        }


        public async Task<bool> UpdateRoomAsync(Room room)
        {
            _context.Rooms.Update(room);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteRoomAsync(Room room)
        {
            _context.Rooms.Remove(room);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Room>> GetRoomsByIdsAsync(List<int> roomIds)
        {
            return await _context.Rooms.Where(r => roomIds.Contains(r.Id)).ToListAsync();
        }

        public async Task UpdateRoomsAsync(List<Room> rooms)
        {
            _context.Rooms.UpdateRange(rooms);
            await _context.SaveChangesAsync();
        }


        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
