namespace Hotel_Booking_App.Models.DTOs.Hotel_Room
{
    public class RoomResponseDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }
        public RoomType Type { get; set; }
        public string Description { get; set; }
        public decimal PricePerNight { get; set; }
        public bool IsAvailable { get; set; }
        public int Capacity { get; set; }
        public int HotelId { get; set; }
    }
}
