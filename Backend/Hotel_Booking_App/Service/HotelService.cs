using Hotel_Booking_App.Interface.Hotel_Room;
using Hotel_Booking_App.Models;
using Hotel_Booking_App.Models.DTOs.Hotel_Room;
using AutoMapper;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;


namespace Hotel_Booking_App.Services
{
    public class HotelService : IHotelService
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IMapper _mapper;
        private readonly BlobServiceClient _blobServiceClient;

        public HotelService(IHotelRepository hotelRepository, IMapper mapper, BlobServiceClient blobServiceClient)
        {
            _hotelRepository = hotelRepository;
            _mapper = mapper;
            _blobServiceClient = blobServiceClient;
        }


       

        public async Task<IEnumerable<HotelResponseDto>> GetAllHotelsAsync()
        {
            var hotels = await _hotelRepository.GetAllHotelsAsync();
            return _mapper.Map<IEnumerable<HotelResponseDto>>(hotels);
        }



        public async Task<HotelResponseDto> AddHotelAsync(int ownerId, AddHotelDto dto)
        {
            var hotel = _mapper.Map<Hotel>(dto);
            hotel.HotelOwnerId = ownerId;

            
            var containerClient = _blobServiceClient.GetBlobContainerClient("hotel-images");
            await containerClient.CreateIfNotExistsAsync();

            for (int i = 0; i < dto.ImageFiles.Count; i++)
            {
                byte[] imageBytes = Convert.FromBase64String(dto.ImageFiles[i].Split(',')[1]);
                string blobName = $"{Guid.NewGuid()}.jpg";
                var blobClient = containerClient.GetBlobClient(blobName);
                using (var stream = new MemoryStream(imageBytes))
                {
                    await blobClient.UploadAsync(stream, true);
                }
                hotel.Images.Add(new HotelImage
                {
                    ImageUrl = blobClient.Uri.ToString(),
                    IsPrimary = i == (dto.PrimaryImageIndex ?? 0) 
                });
            }

            
            if (hotel.Images.Count > 0 && !hotel.Images.Any(img => img.IsPrimary))
            {
                hotel.Images[0].IsPrimary = true; 
            }
            var primaryImages = hotel.Images.Where(img => img.IsPrimary).ToList();
            if (primaryImages.Count > 1)
            {
                foreach (var img in primaryImages.Skip(1))
                {
                    img.IsPrimary = false; 
                }
            }

            
            var uniqueAmenities = dto.Amenities?.Distinct().ToList() ?? new List<string>();
            foreach (var amenity in uniqueAmenities)
            {
                if (!hotel.Amenities.Any(a => a.Name == amenity))
                {
                    hotel.Amenities.Add(new HotelAmenity { Name = amenity });
                }
            }

            await _hotelRepository.AddHotelAsync(hotel);
            return _mapper.Map<HotelResponseDto>(hotel);
        }





        public async Task<IEnumerable<HotelResponseDto>> GetHotelsByOwnerIdAsync(int ownerId)
        {
            var hotels = await _hotelRepository.GetHotelsByOwnerIdAsync(ownerId);
            return _mapper.Map<IEnumerable<HotelResponseDto>>(hotels);
        }

        public async Task<HotelResponseDto?> GetHotelByIdAsync(int hotelId)
        {
            var hotel = await _hotelRepository.GetHotelByIdAsync(hotelId);
            return hotel != null ? _mapper.Map<HotelResponseDto>(hotel) : null;
        }

        public async Task<Hotel?> GetHotelEntityByIdAsync(int hotelId)
        {
            return await _hotelRepository.GetHotelByIdAsync(hotelId); 
        }

        public async Task<List<HotelResponseDto>> SearchHotelsAsync(HotelSearchRequestDto searchParams)
        {
            var hotels = await _hotelRepository.SearchHotelsAsync(searchParams);
            return _mapper.Map<List<HotelResponseDto>>(hotels);
        }





        public async Task<HotelResponseDto> UpdateHotelAsync(int hotelId, int ownerId, AddHotelDto dto)
        {
            var hotel = await _hotelRepository.GetHotelByIdAsync(hotelId);
            if (hotel == null || hotel.HotelOwnerId != ownerId)
            {
                return null; 
            }

            
            hotel.Name = dto.Name ?? hotel.Name;
            hotel.Address = dto.Address ?? hotel.Address;
            hotel.City = dto.City ?? hotel.City;
            hotel.Country = dto.Country ?? hotel.Country;
            hotel.StarRating = dto.StarRating ?? hotel.StarRating;
            hotel.Description = dto.Description ?? hotel.Description;
            hotel.ContactEmail = dto.ContactEmail ?? hotel.ContactEmail;
            hotel.ContactPhone = dto.ContactPhone ?? hotel.ContactPhone;
            hotel.IsActive = dto.IsActive ?? hotel.IsActive;

            
            if (dto.Amenities != null)
            {
                hotel.Amenities.Clear();
                var uniqueAmenities = dto.Amenities.Distinct().ToList();
                foreach (var amenity in uniqueAmenities)
                {
                    hotel.Amenities.Add(new HotelAmenity { Name = amenity });
                }
            }

            
            if (dto.ImageFiles != null && dto.ImageFiles.Count > 0)
            {
                hotel.Images.Clear();
                var containerClient = _blobServiceClient.GetBlobContainerClient("hotel-images");
                await containerClient.CreateIfNotExistsAsync();

                for (int i = 0; i < dto.ImageFiles.Count; i++)
                {
                    string imageUrl;
                    if (dto.ImageFiles[i].StartsWith("data:image")) 
                    {
                        byte[] imageBytes = Convert.FromBase64String(dto.ImageFiles[i].Split(',')[1]);
                        string blobName = $"{Guid.NewGuid()}.jpg";
                        var blobClient = containerClient.GetBlobClient(blobName);
                        using (var stream = new MemoryStream(imageBytes))
                        {
                            await blobClient.UploadAsync(stream, true);
                        }
                        imageUrl = blobClient.Uri.ToString();
                    }
                    else
                    {
                        imageUrl = dto.ImageFiles[i]; 
                    }
                    hotel.Images.Add(new HotelImage
                    {
                        ImageUrl = imageUrl,
                        IsPrimary = i == (dto.PrimaryImageIndex ?? 0)
                    });
                }

                if (!hotel.Images.Any(img => img.IsPrimary))
                {
                    hotel.Images[0].IsPrimary = true;
                }
                var primaryImages = hotel.Images.Where(img => img.IsPrimary).ToList();
                if (primaryImages.Count > 1)
                {
                    foreach (var img in primaryImages.Skip(1))
                    {
                        img.IsPrimary = false;
                    }
                }
            }

            await _hotelRepository.UpdateHotelAsync(hotel);
            return _mapper.Map<HotelResponseDto>(hotel);
        }


        public async Task<bool> DeleteHotelAsync(int hotelId)
        {
            var hotel = await _hotelRepository.GetHotelByIdAsync(hotelId);
            if (hotel == null) return false;

            await _hotelRepository.DeleteHotelAsync(hotelId);
            return true;
        }

        public async Task<List<HotelResponseDto>> SearchHotelsWithRoomsAsync(RoomSearchRequestDto request)
        {
            var hotels = await _hotelRepository.GetHotelsWithRoomsAsync(request.City, request.Country, request.MaxPrice);
            return _mapper.Map<List<HotelResponseDto>>(hotels);
        }

        public async Task<List<string>> GetDistinctCitiesAsync(string? prefix = null)
        {
            var cities = await _hotelRepository.GetDistinctCitiesAsync(prefix);
            return cities;
        }
    }

}