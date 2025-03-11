using Hotel_Booking_App.Contexts;
using Hotel_Booking_App.Interface.Hotel_Room;
using Hotel_Booking_App.Models;
using Hotel_Booking_App.Models.DTOs.Hotel_Room;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Hotel_Booking_App.Repositories
{
    public class HotelRepository : IHotelRepository
    {
        private readonly HotelBookingDbContext _context;

        public HotelRepository(HotelBookingDbContext context)
        {
            _context = context;
        }

        public async Task AddHotelAsync(Hotel hotel)
        {
            await _context.Hotels.AddAsync(hotel);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Hotel>> GetHotelsByOwnerIdAsync(int ownerId)
        {
            return await _context.Hotels
                .Where(h => h.HotelOwnerId == ownerId)
                .Include(h => h.Rooms)      
                .Include(h => h.Images)     
                .Include(h => h.Reviews)
                .Include(h => h.Amenities)  
                .ToListAsync();
        }

        public async Task<List<Room>> GetRoomsByHotelIdAsync(int hotelId)
        {
            return await _context.Rooms
                .Where(r => r.HotelId == hotelId)
                .ToListAsync();
        }

        public async Task<Hotel?> GetHotelByIdAsync(int hotelId)
        {
            return await _context.Hotels
                .Include(h => h.Rooms)
                .Include(h => h.Images)
                .Include(h => h.Amenities)
                .Include(h => h.Reviews)
                .FirstOrDefaultAsync(h => h.Id == hotelId);
        }

        public async Task<IEnumerable<Hotel>> GetAllHotelsAsync()
        {
            return await _context.Hotels
                .Include(h => h.Reviews)
                .ThenInclude(r => r.Customer)
                .ToListAsync();
        }

        public async Task<List<Hotel>> SearchHotelsAsync(HotelSearchRequestDto searchParams)
        {
            var query = _context.Hotels
                .Include(h => h.Rooms)
                .Include(h => h.Images)
                .Include(h => h.Amenities)
                .Include(h => h.Reviews)
                .AsQueryable();

           
            if (!string.IsNullOrEmpty(searchParams.SearchTerm))
            {
                var searchTermLower = searchParams.SearchTerm.ToLower();
                query = query.Where(h =>
                    (h.City != null && h.City.ToLower().Contains(searchTermLower)) ||
                    (h.Country != null && h.Country.ToLower().Contains(searchTermLower)) ||
                    (h.Name != null && h.Name.ToLower().Contains(searchTermLower))
                );
            }

            
            if (!string.IsNullOrEmpty(searchParams.Country))
                query = query.Where(h => h.Country != null && h.Country.ToLower() == searchParams.Country.ToLower());

           
            if (!string.IsNullOrEmpty(searchParams.City))
                query = query.Where(h => h.City != null && h.City.ToLower() == searchParams.City.ToLower());

           
            query = query.Where(h => h.IsActive == true);

            
            var checkInDate = searchParams.GetCheckInDateAsDateTime();
            var checkOutDate = searchParams.GetCheckOutDateAsDateTime();

            if (checkInDate.HasValue && checkOutDate.HasValue)
            {
                
                var bookedRoomIds = await _context.BookingRooms
                    .Where(br => br.Booking.CheckInDate < checkOutDate &&
                                 br.Booking.CheckOutDate > checkInDate)
                    .Select(br => br.RoomId)
                    .ToListAsync();

                
                query = query.Where(h => h.Rooms
                    .Count(r => !bookedRoomIds.Contains(r.Id) && 
                                r.IsAvailable && 
                                r.Capacity >= searchParams.NumberOfGuests && 
                                (searchParams.MaxPrice.HasValue ? r.PricePerNight <= searchParams.MaxPrice : true)) 
                    >= searchParams.NumberOfRooms); 
            }

            
            if (searchParams.Amenities != null && searchParams.Amenities.Any())
                query = query.Where(h => h.Amenities.Any(a => searchParams.Amenities.Contains(a.Name)));

            return await query.ToListAsync();
        }
        public async Task<List<string>> GetDistinctCitiesAsync(string? prefix)
        {
            var query = _context.Hotels
                .Select(h => h.City)
                .Distinct(); 

            if (!string.IsNullOrEmpty(prefix))
            {
                query = query.Where(c => c.ToLower().StartsWith(prefix.ToLower()));
            }

            return await query
                .OrderBy(c => c)
                .ToListAsync();
        }
        public async Task UpdateHotelAsync(Hotel hotel)
        {
            _context.Hotels.Update(hotel);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteHotelAsync(int hotelId)
        {
            var hotel = await GetHotelByIdAsync(hotelId);
            if (hotel != null)
            {
                _context.Hotels.Remove(hotel);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Hotel>> GetHotelsWithRoomsAsync(string city, string country, decimal? maxPrice)
        {
            var query = _context.Hotels
                .Include(h => h.Rooms)
                .Where(h => h.City != null && h.City.ToLower() == city.ToLower() &&
                            h.Country != null && h.Country.ToLower() == country.ToLower() &&
                            h.Rooms.Any(r => r.IsAvailable))
                .AsQueryable();

            if (maxPrice.HasValue)
            {
                query = query.Where(h => h.Rooms.Any(r => r.PricePerNight <= maxPrice.Value));
            }

            return await query.ToListAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}