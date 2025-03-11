using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HotelBookingApp.Models;

namespace Hotel_Booking_App.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Customer")]
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public int HotelId { get; set; }  
        public Hotel Hotel { get; set; }  // Navigation property

        public int RoomId { get; set; }


        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Required]
        public int NumberOfGuests { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Pending; // Default status is Pending

        public string? SpecialRequest { get; set; }

       

        // ✅ Many-to-Many Relationship with Room
        public ICollection<BookingRoom> BookingRooms { get; set; } = new List<BookingRoom>();

        // ✅ One-to-One Relationship with Payment
        public Payment Payment { get; set; }
       
        
    }

    public enum BookingStatus
    {
        Pending,
        Paid,
        Cancelled
        
    }
}
