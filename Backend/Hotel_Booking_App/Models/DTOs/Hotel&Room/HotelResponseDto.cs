using Hotel_Booking_App.Models.DTOs.Review;

namespace Hotel_Booking_App.Models.DTOs.Hotel_Room
{
    public class HotelResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public int? StarRating { get; set; }
        public string Description { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public bool? IsActive { get; set; }
        public int HotelOwnerId { get; set; }

        
        public List<HotelImageDto> Images { get; set; } = new List<HotelImageDto>(); 
        public List<HotelAmenityDto> Amenities { get; set; } = new List<HotelAmenityDto>();
        public List<RoomResponseDto> Rooms { get; set; } = new List<RoomResponseDto>();
        public List<ReviewResponseDto> Reviews { get; set; } = new List<ReviewResponseDto>();
    }

    
    public class HotelImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public int HotelId { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class HotelAmenityDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int HotelId { get; set; }
    }
}
