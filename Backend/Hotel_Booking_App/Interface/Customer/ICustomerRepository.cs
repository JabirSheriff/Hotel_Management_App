using Hotel_Booking_App.Models;
using System.Threading.Tasks;

namespace Hotel_Booking_App.Interface.Customer
{
    public interface ICustomerRepository
    {
        Task<Hotel_Booking_App.Models.Customer?> GetCustomerByIdAsync(int customerId);
        Task<bool> SaveChangesAsync();
    }
}
