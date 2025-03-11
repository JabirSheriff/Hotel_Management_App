using System.ComponentModel.DataAnnotations;

namespace Hotel_Booking_App.Misc
{
    public class CustomStatusValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Status cannot be null");
            }

            if (value.ToString().ToLower() != "active" && value.ToString().ToLower() != "inactive")
            {
                return new ValidationResult("Status must be either 'Active' or 'InActive'");
            }

            return ValidationResult.Success;
        }
    }
}