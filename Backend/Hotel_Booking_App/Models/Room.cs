using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_Booking_App.Models
{
    public class Room
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)] 
        public string RoomNumber { get; set; }

        [Required]
        public RoomType Type { get; set; }

        [Required]
        [StringLength(500)] 
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerNight { get; set; }

        [Required]
        public bool IsAvailable { get; set; } = true;

        [Required]
        [Range(1, 10)] 
        public int Capacity { get; set; }

        [Required]
        [ForeignKey("Hotel")]
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }

        public ICollection<BookingRoom> BookingRooms { get; set; } = new List<BookingRoom>();
    }

    public enum RoomType
    {
        StandardWithBalcony = 1,
        SuperiorWithBalcony = 2,
        PremiumWithBalcony = 3
    }
}