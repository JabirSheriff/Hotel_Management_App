using Hotel_Booking_App.Interface.Hotel_Room;
using Hotel_Booking_App.Models;
using Hotel_Booking_App.Models.DTOs.Booking;
using Hotel_Booking_App.Services;
using HotelBookingApp.Interfaces;
using HotelBookingApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HotelBookingApp.Controllers
{
    [Route("api/booking")]
    [ApiController]
    [EnableCors("AllowAllOrigins")]
    //[Authorize(Roles = "Customer")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IHotelService _hotelService;

        public BookingController(IBookingService bookingService, IHotelService hotelService)
        {
            _bookingService = bookingService;
            _hotelService = hotelService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> CreateBooking([FromBody] BookingRequestDto request)
        {
            try
            {
                var customerId = int.Parse(User.FindFirst("customerId")?.Value ?? throw new UnauthorizedAccessException("Invalid customer ID"));

                
                var roomType = (RoomType)(request.RoomType + 1);

                var booking = await _bookingService.CreateBookingAsync(
                    customerId,
                    request.HotelId,
                    roomType,           
                    request.CheckInDate,
                    request.CheckOutDate,
                    request.NumberOfRooms,
                    request.NumberOfGuests,
                    request.SpecialRequest
                );
                return Ok(booking);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{bookingId}")]
        public async Task<IActionResult> GetBooking(int bookingId)
        {
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null) return NotFound();
            return Ok(booking);
        }

        [HttpGet("customer")]
        public async Task<IActionResult> GetCustomerBookings()
        {
            var customerId = int.Parse(User.FindFirst("customerId")?.Value ?? throw new UnauthorizedAccessException("Invalid customer ID"));
            var bookings = await _bookingService.GetBookingsByCustomerIdAsync(customerId);

            var response = bookings.Select(b => new BookingResponseDto
            {
                BookingId = b.Id,
                HotelId = b.HotelId,
                HotelName = b.Hotel?.Name ?? "Unknown Hotel", 
                RoomsBooked = b.BookingRooms.Select(br => new RoomDetailsDto
                {
                    RoomId = br.RoomId,
                    RoomNumber = br.Room?.RoomNumber,
                    RoomType = br.Room?.Type ?? RoomType.StandardWithBalcony,
                    PricePerNight = br.Room?.PricePerNight ?? 0
                }).ToList(),
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                NumberOfGuests = b.NumberOfGuests,
                TotalPrice = b.TotalPrice,
                Status = b.Status, 
                SpecialRequest = b.SpecialRequest
            }).ToList();

            return Ok(response);
        }

        [HttpGet("by-owner")]
        public async Task<IActionResult> GetBookingsForOwner([FromQuery] string? hotelIds = null)
        {
            try
            {
                var hotelOwnerId = int.Parse(User.FindFirst("HotelOwnerId")?.Value ?? throw new UnauthorizedAccessException("Invalid hotel owner ID"));
                var hotels = await _hotelService.GetHotelsByOwnerIdAsync(hotelOwnerId); 
                var allHotelIds = hotels.Select(h => h.Id).ToList();

                
                var filteredHotelIds = string.IsNullOrEmpty(hotelIds)
                  ? allHotelIds 
                  : hotelIds.Split(',').Select(int.Parse).Where(id => allHotelIds.Contains(id)).ToList();

                var bookings = await _bookingService.GetBookingsByHotelIdsAsync(filteredHotelIds);

                var response = new
                {
                    Hotels = hotels.Select(h => new { h.Id, h.Name }).ToList(), 
                    Bookings = bookings.Select(b => new BookingResponseDto
                    {
                        BookingId = b.Id,
                        HotelId = b.HotelId,
                        HotelName = b.Hotel.Name,
                        RoomsBooked = b.BookingRooms.Select(br => new RoomDetailsDto
                        {
                            RoomId = br.RoomId,
                            RoomNumber = br.Room.RoomNumber,
                            RoomType = br.Room.Type,
                            PricePerNight = br.Room.PricePerNight
                        }).ToList(),
                        RoomsBookedCount = b.BookingRooms.Count,
                        CheckInDate = b.CheckInDate,
                        CheckOutDate = b.CheckOutDate,
                        NumberOfGuests = b.NumberOfGuests,
                        TotalPrice = b.TotalPrice,
                        Status = b.Status,
                        SpecialRequest = b.SpecialRequest,
                        CustomerName = b.Customer?.User?.FullName
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{bookingId}")]
        public async Task<IActionResult> UpdateBooking(int bookingId, DateTime checkInDate, DateTime checkOutDate, int numberOfGuests, string? specialRequest)
        {
            try
            {
                var success = await _bookingService.UpdateBookingAsync(bookingId, checkInDate, checkOutDate, numberOfGuests, specialRequest);
                if (!success) return NotFound();
                return Ok("Booking updated");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{bookingId}")]
        public async Task<IActionResult> DeleteBooking(int bookingId)
        {
            var success = await _bookingService.DeleteBookingAsync(bookingId);
            if (!success) return NotFound();
            return Ok("Booking deleted");
        }
    }
}