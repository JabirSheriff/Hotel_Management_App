using Hotel_Booking_App.Models;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Hotel_Booking_App.Models.DTOs.Hotel_Room
{
    public class HotelSearchRequestDto
    {
        public string? SearchTerm { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? CheckInDate { get; set; }
        public string? CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public int NumberOfRooms { get; set; }
        public RoomType? RoomType { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<string>? Amenities { get; set; }

        
        public DateTime? GetCheckInDateAsDateTime()
        {
            if (string.IsNullOrEmpty(CheckInDate))
                return null;

            return DateTime.TryParseExact(
                CheckInDate,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date)
                ? date
                : null;
        }

        
        public DateTime? GetCheckOutDateAsDateTime()
        {
            if (string.IsNullOrEmpty(CheckOutDate))
                return null;

            return DateTime.TryParseExact(
                CheckOutDate,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date)
                ? date
                : null;
        }
    }
}