using Hotel_Booking_App.Models;
using Hotel_Booking_App.Models.DTOs.Hotel_Room;

namespace Hotel_Booking_App.Interface.Hotel_Room
{
    public interface IHotelRepository
    {
        Task AddHotelAsync(Hotel hotel);
        Task<IEnumerable<Hotel>> GetHotelsByOwnerIdAsync(int ownerId);
        Task<Hotel?> GetHotelByIdAsync(int hotelId);
        Task<IEnumerable<Hotel>> GetAllHotelsAsync();
        Task<List<string>> GetDistinctCitiesAsync(string? prefix);
        Task<List<Hotel>> SearchHotelsAsync(HotelSearchRequestDto searchParams); 
        Task UpdateHotelAsync(Hotel hotel);
        Task DeleteHotelAsync(int hotelId);
        Task<List<Room>> GetRoomsByHotelIdAsync(int hotelId);
        Task<List<Hotel>> GetHotelsWithRoomsAsync(string city, string country, decimal? maxPrice);
        Task<bool> SaveChangesAsync();
    }
}
