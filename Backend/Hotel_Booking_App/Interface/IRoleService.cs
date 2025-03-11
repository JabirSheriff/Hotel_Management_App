using Hotel_Booking_App.Models.DTOs;

namespace Hotel_Booking_App.Interface
{
    public interface IRoleService
    {
        Task<bool> UpdateUserRoleAsync(UpdateUserRoleDto dto);
    }
}
