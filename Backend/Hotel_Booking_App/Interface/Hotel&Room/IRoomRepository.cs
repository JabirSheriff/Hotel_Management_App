using System;
using Hotel_Booking_App.Models;

namespace Hotel_Booking_App.Interface.Hotel_Room
{
    public interface IRoomRepository
    {
        Task<bool> AddRoomAsync(Room room);
        Task<Room?> GetRoomByIdAsync(int roomId);
        Task<IEnumerable<Room>> GetRoomsByHotelIdAsync(int hotelId);
        Task<bool> UpdateRoomAsync(Room room);
        Task<bool> DeleteRoomAsync(Room room);
        Task<List<Room>> GetAvailableRoomsAsync(int hotelId, int roomType, DateTime checkInDate, DateTime checkOutDate);
        Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkInDate, DateTime checkOutDate);
        Task<List<Room>> GetRoomsByIdsAsync(List<int> roomIds);
        Task UpdateRoomsAsync(List<Room> rooms);
        Task<bool> SaveChangesAsync();
    }

}
