using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Hotel_Booking_App.Interface;
using Hotel_Booking_App.Models.DTOs;
using Hotel_Booking_App.Models;
using Microsoft.IdentityModel.Tokens;

namespace Hotel_Booking_App.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<UserRegistrationResponseDto> RegisterAsync(UserRegistrationDto userregistrationDto)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(userregistrationDto.Email);
            if (existingUser != null)
                throw new Exception("User already exists.");

            using var hmac = new HMACSHA512();
            var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userregistrationDto.Password));
            var passwordSalt = hmac.Key;

            var user = new User
            {
                FullName = userregistrationDto.FullName,
                Email = userregistrationDto.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                PhoneNumber = userregistrationDto.PhoneNumber,
                Role = "Unassigned" 
            };

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            return new UserRegistrationResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                PhoneNumber = user.PhoneNumber,
            };
        }

        

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetUserByEmailAsync(loginDto.Email);
            if (user == null)
                throw new Exception("User not found.");

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            if (!computedHash.SequenceEqual(user.PasswordHash))
                throw new Exception("Invalid password.");

           
            if (user.Role == "Unassigned")
            {
                return new LoginResponseDto
                {
                    Message = "Login successful, but role is not assigned. Please contact Admin.",
                    Token = null,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role
                };
            }

           
            int? hotelOwnerId = null;
            if (user.Role == "HotelOwner")
            {
                var hotelOwner = await _userRepository.GetHotelOwnerByUserIdAsync(user.Id);
                hotelOwnerId = hotelOwner?.Id;
            }

            
            int? customerId = null;
            if (user.Role == "Customer")
            {
                var customer = await _userRepository.GetCustomerByUserIdAsync(user.Id);
                customerId = customer?.Id;
            }

            
            var secretKey = _configuration["Jwt:Secret"];
            if (string.IsNullOrEmpty(secretKey))
                throw new Exception("JWT Secret Key is missing in configuration.");

            var key = Encoding.UTF8.GetBytes(secretKey);

            
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };

            if (hotelOwnerId.HasValue)
                claims.Add(new Claim("hotelOwnerId", hotelOwnerId.Value.ToString()));
            if (customerId.HasValue)
                claims.Add(new Claim("customerId", customerId.Value.ToString()));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            string jwtToken = tokenHandler.WriteToken(token);

            
            string welcomeMessage = user.Role switch
            {
                "Admin" => "Welcome to Admin Dashboard",
                "Customer" => "Welcome to Hotel Booking App",
                "HotelOwner" => "Welcome to Hotel Booking Dashboard",
                _ => "Unauthorized access"
            };

            return new LoginResponseDto
            {
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                HotelOwnerId = hotelOwnerId,  
                CustomerId = customerId, 
                Message = welcomeMessage,
                Token = jwtToken
            };
        }


        public async Task<UserRegistrationResponseDto> RegisterCustomerAsync(UserRegistrationDto userRegistrationDto)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(userRegistrationDto.Email);
            if (existingUser != null)
                throw new Exception("User already exists.");

            using var hmac = new HMACSHA512();
            var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userRegistrationDto.Password));
            var passwordSalt = hmac.Key;

            var user = new User
            {
                FullName = userRegistrationDto.FullName,
                Email = userRegistrationDto.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                PhoneNumber = userRegistrationDto.PhoneNumber,
                Role = "Customer"  
            };

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            var customer = new Customer
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
            };

            await _userRepository.AddCustomerAsync(customer);
            await _userRepository.SaveChangesAsync();

            // Generate JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _configuration["Jwt:Secret"];
            if (string.IsNullOrEmpty(secretKey))
                throw new Exception("JWT Secret Key is missing in configuration.");

            var key = Encoding.UTF8.GetBytes(secretKey);
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim("customerId", customer.Id.ToString()) 
    };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            string jwtToken = tokenHandler.WriteToken(token);

            return new UserRegistrationResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                PhoneNumber = user.PhoneNumber,
                Token = jwtToken 
            };
        }


        public async Task<bool> UpdateUserProfileAsync(int userId, UpdateUserDto updateUserDto)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return false;

            
            if (!string.IsNullOrWhiteSpace(updateUserDto.FullName) && updateUserDto.FullName != "string" &&updateUserDto.FullName != "")
                user.FullName = updateUserDto.FullName;

            if (!string.IsNullOrWhiteSpace(updateUserDto.PhoneNumber) && updateUserDto.PhoneNumber != "string" && updateUserDto.FullName != "")
                user.PhoneNumber = updateUserDto.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(updateUserDto.Password) && updateUserDto.Password != "string" && updateUserDto.FullName != "")
            {
                using var hmac = new HMACSHA512();
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(updateUserDto.Password));
                user.PasswordSalt = hmac.Key;
            }

            
            if (user.Role == "Customer")
            {
                var customer = await _userRepository.GetCustomerByUserIdAsync(userId);
                if (customer != null)
                {
                    if (!string.IsNullOrWhiteSpace(updateUserDto.FullName) && updateUserDto.FullName != "string")
                        customer.FullName = user.FullName;

                    if (!string.IsNullOrWhiteSpace(updateUserDto.PhoneNumber) && updateUserDto.PhoneNumber != "string")
                        customer.PhoneNumber = user.PhoneNumber;
                }
            }

            
            if (user.Role == "HotelOwner")
            {
                var hotelOwner = await _userRepository.GetHotelOwnerByUserIdAsync(userId);
                if (hotelOwner != null)
                {
                    if (!string.IsNullOrWhiteSpace(updateUserDto.FullName) && updateUserDto.FullName != "string")
                        hotelOwner.FullName = user.FullName;

                    if (!string.IsNullOrWhiteSpace(updateUserDto.PhoneNumber) && updateUserDto.PhoneNumber != "string")
                        hotelOwner.PhoneNumber = user.PhoneNumber;
                }
            }

            await _userRepository.SaveChangesAsync();
            return true;
        }






    }
}
