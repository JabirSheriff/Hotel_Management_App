using Hotel_Booking_App.Models.DTOs;
using System.Threading.Tasks;

namespace Hotel_Booking_App.Interface
{
    public interface IAuthService
    {
        Task<UserRegistrationResponseDto> RegisterAsync(UserRegistrationDto userregistrationDto);
        Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
        Task<UserRegistrationResponseDto> RegisterCustomerAsync(UserRegistrationDto userregistrationDto);
        Task<bool> UpdateUserProfileAsync(int userId, UpdateUserDto updateUserDto);

    }
}
