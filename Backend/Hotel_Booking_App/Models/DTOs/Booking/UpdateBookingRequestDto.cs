namespace Hotel_Booking_App.Models.DTOs.Booking
{
    public class UpdateBookingRequestDto
    {
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public string SpecialRequest { get; set; }
    }
}
