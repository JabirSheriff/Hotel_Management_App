using Hotel_Booking_App.Models;

namespace Hotel_Booking_App.Interface.Review
{
    public interface ICustomerService
    {
        Task<Hotel_Booking_App.Models.Customer> GetCustomerByUserIdAsync(int userId);
    }
}
