using System;
using System.Collections.Generic;
using Hotel_Booking_App.Models;

namespace Hotel_Booking_App.Models.DTOs.Booking
{
    public class BookingResponseDto
    {
        public int BookingId { get; set; }
        public int HotelId { get; set; }
        public string HotelName { get; set; }
        public List<RoomDetailsDto> RoomsBooked { get; set; }
        public int RoomsBookedCount { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        public string? SpecialRequest { get; set; }
        public string CustomerName { get; set; }
    }

    public class RoomDetailsDto
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; }
        public RoomType RoomType { get; set; }
        public decimal PricePerNight { get; set; }
    }
}
