using Hotel_Booking_App.Models;
using System.ComponentModel.DataAnnotations;

namespace Hotel_Booking_App.Interface.Hotel_Room
{
    public class RoomRequestDto
    {
        [Required]
        [StringLength(10)]
        public string RoomNumber { get; set; }

        [Required]
        public RoomType Type { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal PricePerNight { get; set; }

        [Required]
        public bool IsAvailable { get; set; } = true;

        [Required]
        [Range(1, 10, ErrorMessage = "Capacity must be between 1 and 10")]
        public int Capacity { get; set; }

        [Required]
        public int HotelId { get; set; }  
    }
}
