namespace Hotel_Booking_App.Models.DTOs.Review
{
    public class ReviewResponseDto
    {
        public int ReviewId { get; set; }
        public int HotelId { get; set; }
        public string CustomerName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
