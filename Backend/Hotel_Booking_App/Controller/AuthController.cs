using System.Security.Claims;
using Hotel_Booking_App.Interface;
using Hotel_Booking_App.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_Booking_App.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAllOrigins")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;

        public AuthController(IAuthService authService, IUserRepository userRepository)
        {
            _authService = authService;
            _userRepository = userRepository;
        }

        [HttpPost("register User")]
        [AllowAnonymous] 
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto userregistrationDto)
        {
            try
            {
                var userResponse = await _authService.RegisterAsync(userregistrationDto);
                return Ok(userResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Register: {ex}");
                return BadRequest(new { error = "Registration failed. Please try again later." });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous] 
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var response = await _authService.LoginAsync(loginDto);

                if (response == null)
                    return Unauthorized(new { error = "Invalid email or password" }); 

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Login: {ex}"); 
                return BadRequest(new { error = "Login failed. Please try again later." });
            }
        }

        [HttpPost("register-customer")]
        [AllowAnonymous] 
        public async Task<IActionResult> RegisterCustomer([FromBody] UserRegistrationDto userRegistrationDto)
        {
            try
            {
                var userResponse = await _authService.RegisterCustomerAsync(userRegistrationDto);
                return Ok(userResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Register Customer: {ex}");
                return BadRequest(new { error = "Customer registration failed. Please try again later." });
            }
        }

        [HttpGet("users")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userRepository.GetAllUsersAsync();
                var response = users.Select(u => new UserRegistrationResponseDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role,
                    PhoneNumber = u.PhoneNumber
                });
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetAllUsers: {ex}");
                return StatusCode(500, new { error = "Failed to fetch users." });
            }
        }


        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                
                foreach (var claim in User.Claims)
                {
                    Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
                }

                
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                                  User.FindFirst("nameId")?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { error = "User ID claim is missing in token." });

                var userId = int.Parse(userIdClaim);

                var result = await _authService.UpdateUserProfileAsync(userId, updateUserDto);
                if (!result) return BadRequest(new { error = "Profile update failed." });

                return Ok(new { message = "Profile updated successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] UpdateProfile: {ex}");
                return StatusCode(500, new { error = "An error occurred while updating profile." });
            }
        }


    }
}
