namespace Hotel_Booking_App.Models.DTOs
{
    public class UserRegistrationResponseDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string PhoneNumber { get; set; }
        public string Token { get; set; }

    }
}
