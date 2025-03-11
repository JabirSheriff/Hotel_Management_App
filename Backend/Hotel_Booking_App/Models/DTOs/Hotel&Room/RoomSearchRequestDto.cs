namespace Hotel_Booking_App.Models.DTOs.Hotel_Room
{
    public class RoomSearchRequestDto
    {
        public string City { get; set; }
        public string Country { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}
