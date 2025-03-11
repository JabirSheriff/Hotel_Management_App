using Hotel_Booking_App.Models;
using HotelBookingApp.Models;
using System.ComponentModel.DataAnnotations;

public class Hotel
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Country { get; set; }

    [Range(1, 5, ErrorMessage = "Star Rating must be between 1 and 5")]
    public int? StarRating { get; set; }
    public string Description { get; set; }

    [Required]
    public int HotelOwnerId { get; set; }  
    public HotelOwner HotelOwner { get; set; }

    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public bool? IsActive { get; set; }


    // ✅ Navigation Properties
    public List<Room> Rooms { get; set; } = new List<Room>();
    public List<HotelImage> Images { get; set; } = new List<HotelImage>(); 
    public List<HotelAmenity> Amenities { get; set; } = new List<HotelAmenity>(); 
    public List<Review> Reviews { get; set; } = new List<Review>();
    public List<Booking> Bookings { get; set; } = new List<Booking>();

}

// New class for hotel images
public class HotelImage
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } // Will store Azure Blob URL 
    public int HotelId { get; set; }
    public Hotel Hotel { get; set; }
    public bool IsPrimary { get; set; } // Mark one as main image
}

// New class for hotel amenities
public class HotelAmenity
{
    public int Id { get; set; }
    public string Name { get; set; } 
    public int HotelId { get; set; }
    public Hotel Hotel { get; set; }
}
