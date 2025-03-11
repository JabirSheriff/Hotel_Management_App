using System.Threading.Tasks;
using Hotel_Booking_App.Interface.Hotel_Room;
using Hotel_Booking_App.Models.DTOs.Hotel_Room;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_Booking_App.Controllers
{
    //[Authorize(Roles = "HotelOwner")]
    [Route("api/rooms")]
    [EnableCors("AllowAllOrigins")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RoomController(IRoomService roomService, IHttpContextAccessor httpContextAccessor)
        {
            _roomService = roomService;
            _httpContextAccessor = httpContextAccessor;
        }

        // ✅ Add Room
        [HttpPost("add-room")]
        public async Task<IActionResult> AddRoom([FromBody] RoomRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var hotelOwnerIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("hotelOwnerId")?.Value;
            if (string.IsNullOrEmpty(hotelOwnerIdClaim) || !int.TryParse(hotelOwnerIdClaim, out int hotelOwnerId))
                return Unauthorized("Hotel Owner ID claim not found in token.");

            var room = await _roomService.AddRoomAsync(hotelOwnerId, dto);
            return CreatedAtAction(nameof(GetRoomsByHotelId), new { hotelId = room.HotelId }, room);
        }

        // ✅ Get Rooms by Hotel ID
        [HttpGet("get-rooms/{hotelId}")]
        public async Task<IActionResult> GetRoomsByHotelId(int hotelId)
        {
            var rooms = await _roomService.GetRoomsByHotelIdAsync(hotelId);
            return Ok(rooms);
        }

        // ✅ Update Room
        [HttpPatch("update-room/{roomId}")]
        public async Task<IActionResult> UpdateRoom(int roomId,
    [FromBody] JsonPatchDocument<RoomRequestDto> patchDto)
        {
            var ownerId = int.Parse(User.FindFirst("hotelOwnerId")?.Value);
            var updatedRoom = await _roomService.UpdateRoomAsync(ownerId, roomId, patchDto);

            if (updatedRoom == null)
                return BadRequest("Failed to update the room.");

            return Ok(updatedRoom);
        }



        // ✅ Delete Room
        [HttpDelete("delete-room/{roomId}")]
        public async Task<IActionResult> DeleteRoom(int roomId)
        {
            var hotelOwnerIdClaim = User.FindFirst("hotelOwnerId")?.Value;
            if (string.IsNullOrEmpty(hotelOwnerIdClaim) || !int.TryParse(hotelOwnerIdClaim, out int hotelOwnerId))
                return Unauthorized("Invalid token or HotelOwnerId missing.");

            bool isDeleted = await _roomService.DeleteRoomAsync(hotelOwnerId, roomId);
            return isDeleted ? Ok("Room deleted successfully.") : NotFound("Room not found or unauthorized.");
        }
    }
}
