namespace Hotel_Booking_App.Models.DTOs.Hotel_Room
{
    public class HotelWithRoomsResponseDto
    {
        public int HotelId { get; set; }
        public string HotelName { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public List<RoomResponseDto> Rooms { get; set; }
    }
}
