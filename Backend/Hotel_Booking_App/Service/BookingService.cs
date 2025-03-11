using Hotel_Booking_App.Models;
using HotelBookingApp.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HotelBookingApp.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;

        public BookingService(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<Booking> CreateBookingAsync(int customerId, int hotelId, RoomType roomType, DateTime checkIn, DateTime checkOut, int numberOfRooms, int numberOfGuests, string? specialRequest)
        {
            if (checkIn >= checkOut)
                throw new ArgumentException("Check-out date must be after check-in date.");

            
            var allRoomsOfType = await _bookingRepository.GetRoomsByHotelAndTypeAsync(hotelId, roomType);
            if (!allRoomsOfType.Any())
                throw new Exception($"No rooms of type '{roomType}' exist for this hotel.");

            
            if (allRoomsOfType.All(r => r.Capacity < numberOfGuests))
                throw new Exception($"Guest capacity ({numberOfGuests}) exceeds the limit for '{roomType}' rooms (max {allRoomsOfType.Max(r => r.Capacity)}).");

            
            var availableRooms = await _bookingRepository.GetAvailableRoomsAsync(hotelId, numberOfRooms, roomType, numberOfGuests, checkIn, checkOut);
            if (availableRooms.Count < numberOfRooms)
            {
                var bookedUntil = (await _bookingRepository.GetBookingsByCustomerIdAsync(customerId))
                    .Where(b => b.HotelId == hotelId &&
                                b.BookingRooms.Any(br => br.Room.Type == roomType) &&
                                b.CheckInDate < checkOut &&
                                b.CheckOutDate > checkIn)
                    .OrderBy(b => b.CheckOutDate)
                    .FirstOrDefault()?.CheckOutDate;
                var message = bookedUntil.HasValue
                    ? $"No rooms available. '{roomType}' booked until {bookedUntil.Value:yyyy-MM-dd}."
                    : $"No '{roomType}' rooms available for selected dates. Only {availableRooms.Count} available.";
                throw new Exception(message);
            }

            var booking = new Booking
            {
                CustomerId = customerId,
                HotelId = hotelId,
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                NumberOfGuests = numberOfGuests,
                TotalPrice = availableRooms.Take(numberOfRooms).Sum(r => r.PricePerNight) * (checkOut - checkIn).Days,
                SpecialRequest = specialRequest,
                Status = BookingStatus.Pending, 
                BookingRooms = new List<BookingRoom>()
            };

            foreach (var room in availableRooms.Take(numberOfRooms))
            {
                booking.BookingRooms.Add(new BookingRoom
                {
                    RoomId = room.Id,
                    CheckInDate = checkIn,
                    CheckOutDate = checkOut
                });
            }

            if (booking.BookingRooms.Count == 0)
                throw new Exception("No valid rooms assigned to booking.");

            return await _bookingRepository.AddBookingAsync(booking);
        }

        public async Task<Booking?> GetBookingByIdAsync(int bookingId)
        {
            return await _bookingRepository.GetBookingByIdAsync(bookingId);
        }

        public async Task<List<Booking>> GetBookingsByCustomerIdAsync(int customerId)
        {
            return await _bookingRepository.GetBookingsByCustomerIdAsync(customerId);
        }

        public async Task<bool> UpdateBookingAsync(int bookingId, DateTime checkIn, DateTime checkOut, int numberOfGuests, string? specialRequest)
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
            if (booking == null) return false;

            if (checkIn >= checkOut)
                throw new ArgumentException("Check-out date must be after check-in date.");

            var availableRooms = await _bookingRepository.GetAvailableRoomsAsync(
                booking.HotelId,
                booking.BookingRooms.Count(),
                booking.BookingRooms.First().Room.Type,
                numberOfGuests,
                checkIn,
                checkOut
            );
            if (availableRooms.Count < booking.BookingRooms.Count())
                throw new Exception("Rooms not available for updated dates.");

            booking.CheckInDate = checkIn;
            booking.CheckOutDate = checkOut;
            booking.NumberOfGuests = numberOfGuests;
            booking.SpecialRequest = specialRequest;
            booking.TotalPrice = availableRooms.First().PricePerNight * (checkOut - checkIn).Days;

            return await _bookingRepository.UpdateBookingAsync(booking);
        }

        public async Task<bool> DeleteBookingAsync(int bookingId)
        {
            return await _bookingRepository.DeleteBookingAsync(bookingId);
        }



        public async Task<List<Booking>> GetBookingsByHotelIdsAsync(List<int> hotelIds)
        {
            return await _bookingRepository.GetBookingsByHotelIdsAsync(hotelIds);
        }
    }
}