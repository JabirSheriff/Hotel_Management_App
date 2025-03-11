using Bogus;
using Hotel_Booking_App.Contexts;
using Hotel_Booking_App.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Booking_App
{
    public class DatabaseSeeder
    {
        private readonly HotelBookingDbContext _context;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly Faker _faker;

        public DatabaseSeeder(HotelBookingDbContext context, BlobServiceClient blobServiceClient)
        {
            _context = context;
            _blobServiceClient = blobServiceClient;
            _faker = new Faker("en");
        }

        public async Task SeedAsync()
        {
            Console.WriteLine("Starting database seeding...");

           
            var imageFiles = Directory.GetFiles("Images", "*.jpg");
            Console.WriteLine($"Found {imageFiles.Length} images in Images/ folder.");
            if (imageFiles.Length == 0)
            {
                Console.WriteLine("No images found in Images/ folder. Please add some .jpg files.");
                return;
            }
            var imageUrls = await UploadSampleImages(imageFiles);

           
            if (!_context.Users.Any(u => u.Role == "HotelOwner"))
            {
                Console.WriteLine("Seeding hotel owner users...");
                var hotelOwnerUsers = new List<User>();
                for (int i = 0; i < 10; i++) 
                {
                    var (passwordHash, passwordSalt) = CreatePasswordHash("Owner@123");
                    hotelOwnerUsers.Add(new User
                    {
                        // Remove Id = i
                        FullName = _faker.Name.FullName(),
                        Email = _faker.Internet.Email(),
                        PasswordHash = passwordHash,
                        PasswordSalt = passwordSalt,
                        PhoneNumber = _faker.Phone.PhoneNumber("##########"),
                        Role = "HotelOwner",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
                await _context.Users.AddRangeAsync(hotelOwnerUsers);
                await _context.SaveChangesAsync();
                Console.WriteLine("Seeded 10 hotel owner users.");

                
                Console.WriteLine("Seeding hotel owners...");
                var hotelOwners = hotelOwnerUsers.Select(u => new HotelOwner
                {
                    UserId = u.Id, 
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber
                }).ToList();
                await _context.HotelOwners.AddRangeAsync(hotelOwners);
                await _context.SaveChangesAsync();
                Console.WriteLine("Seeded 10 hotel owners.");
            }
            else
            {
                Console.WriteLine("Hotel owners already exist—skipping user seeding.");
            }

            
            if (_context.Hotels.Any())
            {
                Console.WriteLine("Hotels already exist—skipping hotel seeding.");
                return;
            }
            Console.WriteLine("Seeding hotels...");

           
            var ownerIds = await _context.HotelOwners.Select(o => o.Id).ToListAsync();

            
            var cities = new List<(string City, string State)>
{
    ("Amaravati", "Andhra Pradesh"), ("Itanagar", "Arunachal Pradesh"), ("Dispur", "Assam"),
    ("Patna", "Bihar"), ("Raipur", "Chhattisgarh"), ("Panaji", "Goa"),
    ("Gandhinagar", "Gujarat"), ("Chandigarh", "Haryana"), ("Shimla", "Himachal Pradesh"),
    ("Ranchi", "Jharkhand"), ("Bengaluru", "Karnataka"), ("Thiruvananthapuram", "Kerala"),
    ("Bhopal", "Madhya Pradesh"), ("Mumbai", "Maharashtra"), ("Imphal", "Manipur"),
    ("Shillong", "Meghalaya"), ("Aizawl", "Mizoram"), ("Kohima", "Nagaland"),
    ("Bhubaneswar", "Odisha"), ("Chandigarh", "Punjab"), ("Jaipur", "Rajasthan"),
    ("Gangtok", "Sikkim"), ("Chennai", "Tamil Nadu"), ("Hyderabad", "Telangana"),
    ("Agartala", "Tripura"), ("Lucknow", "Uttar Pradesh"), ("Dehradun", "Uttarakhand"),
    ("Kolkata", "West Bengal"), ("Delhi", "Delhi")
};


           
            var amenitiesList = new[] { "Wi-Fi", "Pool", "Gym", "Restaurant", "Parking", "Spa" };

            var hotels = new List<Hotel>();
            Random random = new Random();

            foreach (var (city, state) in cities)
            {
                for (int i = 0; i < 5; i++) 
                {
                    var hotel = new Hotel
                    {
                        Name = $"{_faker.Company.CompanyName()} Hotel",
                        Address = $"{_faker.Address.StreetAddress()}, {state}",
                        City = city,
                        Country = "India",
                        StarRating = random.Next(3, 6),
                        Description = _faker.Lorem.Paragraph(),
                        ContactEmail = _faker.Internet.Email(),
                        ContactPhone = _faker.Phone.PhoneNumber("##########"),
                        IsActive = true,
                        HotelOwnerId = ownerIds[random.Next(ownerIds.Count)]
                    };

                   
                    int imageCount = random.Next(5, 6);
                    for (int j = 0; j < imageCount; j++)
                    {
                        hotel.Images.Add(new HotelImage
                        {
                            ImageUrl = imageUrls[random.Next(imageUrls.Count)],
                            IsPrimary = j == 0
                        });
                    }

                    
                    var amenities = _faker.Random.Shuffle(amenitiesList).Take(random.Next(2, 5)).ToList();
                    foreach (var amenity in amenities)
                    {
                        hotel.Amenities.Add(new HotelAmenity { Name = amenity });
                    }

                   
                    int roomCount = random.Next(5, 11);
                    for (int k = 1; k <= roomCount; k++)
                    {
                        hotel.Rooms.Add(new Room
                        {
                            RoomNumber = $"{k + 100}",
                            Type = (RoomType)random.Next(1, 4),
                            Description = $"{_faker.Lorem.Sentence()} with Balcony",
                            PricePerNight = random.Next(1000, 8000),
                            IsAvailable = _faker.Random.Bool(0.8f),
                            Capacity = random.Next(2, 5)
                        });
                    }

                    hotels.Add(hotel);
                }
            }

            await _context.Hotels.AddRangeAsync(hotels);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Seeded {hotels.Count} hotels with rooms, images, and amenities.");
        }

        private async Task<List<string>> UploadSampleImages(string[] imagePaths)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient("hotel-images");
            await containerClient.CreateIfNotExistsAsync();
            var uploadedUrls = new List<string>();

            foreach (var path in imagePaths)
            {
                byte[] imageBytes = File.ReadAllBytes(path);
                string blobName = $"{Guid.NewGuid()}.jpg"; 
                var blobClient = containerClient.GetBlobClient(blobName);
                using (var stream = new MemoryStream(imageBytes))
                {
                    await blobClient.UploadAsync(stream, true);
                }
                uploadedUrls.Add(blobClient.Uri.ToString());
                Console.WriteLine($"Uploaded image: {blobClient.Uri}");
            }

            return uploadedUrls;
        }

        private (byte[] PasswordHash, byte[] PasswordSalt) CreatePasswordHash(string password)
        {
            using var hmac = new HMACSHA512();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return (hash, salt);
        }
    }
}