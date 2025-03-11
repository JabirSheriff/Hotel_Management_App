using System.ComponentModel.DataAnnotations;

namespace Hotel_Booking_App.Models.DTOs.Hotel_Room
{
    public class AddHotelDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Country { get; set; }

        [Range(1, 5, ErrorMessage = "Star Rating must be between 1 and 5")]
        public int? StarRating { get; set; }

        public string Description { get; set; }

        [EmailAddress]
        public string? ContactEmail { get; set; }

        [Phone]
        public string? ContactPhone { get; set; }

        public bool? IsActive { get; set; }
        public int? PrimaryImageIndex { get; set; }

        
        public List<string> ImageFiles { get; set; } = new List<string>(); 
        public List<string> Amenities { get; set; } = new List<string>();
    }
}
