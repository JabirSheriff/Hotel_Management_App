using HotelBookingApp.Models;

namespace Hotel_Booking_App.Models.DTOs.Payment
{
    public class PaymentResponseDto
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string PaymentStatus { get; set; } 

        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }

        public decimal RoomPricePerNight { get; set; }
    }
}
