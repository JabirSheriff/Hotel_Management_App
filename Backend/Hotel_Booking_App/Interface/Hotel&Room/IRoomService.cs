using Hotel_Booking_App.Models.DTOs.Hotel_Room;
using Microsoft.AspNetCore.JsonPatch;

namespace Hotel_Booking_App.Interface.Hotel_Room
{
    public interface IRoomService
    {
        Task<RoomResponseDto> AddRoomAsync(int ownerId, RoomRequestDto dto);
        Task<IEnumerable<RoomResponseDto>> GetRoomsByHotelIdAsync(int hotelId);
        Task<RoomResponseDto> UpdateRoomAsync(int ownerId, int roomId, JsonPatchDocument<RoomRequestDto> patchDto);
        Task<bool> DeleteRoomAsync(int ownerId, int roomId);
    }
}
