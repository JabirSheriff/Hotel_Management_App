namespace Hotel_Booking_App.Models.DTOs
{
    public class LoginResponseDto
    {
        public string FullName { get; set; }  
        public string Email { get; set; }  
        public string Role { get; set; }  
        public string Message { get; set; }
        public string Token { get; set; }
        public int? HotelOwnerId { get; set; }
        public int? CustomerId { get; set; }
    }
}
