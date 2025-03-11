using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Hotel_Booking_App.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } 


        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Full Name must be between 3 and 100 characters.")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public byte[] PasswordHash { get; set; }

        public byte[] PasswordSalt { get; set; }

        [Phone]
        [MaxLength(10)]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Unassigned";  // Default to "Unassigned"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ✅ Navigation Properties
        public Customer Customer { get; set; }
        public HotelOwner HotelOwner { get; set; }
    }
}
