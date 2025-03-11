using System.ComponentModel.DataAnnotations;

namespace Hotel_Booking_App.Models.DTOs.Review
{
    public class ReviewRequestDto
    {
        [Required]
        public int HotelId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters.")]
        public string Comment { get; set; }
    }
}
