using Hotel_Booking_App.Models;
using HotelBookingApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelBookingApp.Interfaces
{
    public interface IBookingRepository
    {
        Task<Booking> AddBookingAsync(Booking booking);
        Task<Booking?> GetBookingByIdAsync(int bookingId);
        Task<List<Booking>> GetBookingsByCustomerIdAsync(int customerId);
        Task<List<Room>> GetAvailableRoomsAsync(int hotelId, int numberOfRooms, RoomType roomType, int numberOfGuests, DateTime checkIn, DateTime checkOut); // Updated to 6 args
        Task<bool> UpdateBookingAsync(Booking booking);
        Task<bool> DeleteBookingAsync(int bookingId);
        Task<List<Room>> GetRoomsByHotelAndTypeAsync(int hotelId, RoomType roomType);
        Task<List<Booking>> GetBookingsByHotelIdsAsync(List<int> hotelIds);

    }
}