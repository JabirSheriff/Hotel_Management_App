using Hotel_Booking_App.Models;

namespace HotelBookingApp.Interfaces
{
    public interface IBookingService
    {
        Task<Booking> CreateBookingAsync(int customerId, int hotelId, RoomType roomType, DateTime checkIn, DateTime checkOut, int numberOfRooms, int numberOfGuests, string? specialRequest);
        Task<Booking?> GetBookingByIdAsync(int bookingId);
        Task<List<Booking>> GetBookingsByCustomerIdAsync(int customerId);
        Task<bool> UpdateBookingAsync(int bookingId, DateTime checkIn, DateTime checkOut, int numberOfGuests, string? specialRequest);
        Task<bool> DeleteBookingAsync(int bookingId);
        Task<List<Booking>> GetBookingsByHotelIdsAsync(List<int> hotelIds);
    }
}