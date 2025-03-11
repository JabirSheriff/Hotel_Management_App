using HotelBookingApp.Models;

namespace Hotel_Booking_App.Models.DTOs.Payment
{
    public class PaymentRequestDto
    {
        public int BookingId { get; set; }
        public PaymentMethod PaymentMethod { get; set; } 
    }
}
