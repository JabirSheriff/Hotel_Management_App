using Azure;
using Hotel_Booking_App.Interface.Hotel_Room;
using Hotel_Booking_App.Models.DTOs.Hotel_Room;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Cors;
using HotelBookingApp.Interfaces;

namespace Hotel_Booking_App.Controllers
{
    [Route("api/hotels")]
    [ApiController]
    [EnableCors("AllowAllOrigins")]
    //[Authorize(Roles = "HotelOwner")]
    public class HotelController : ControllerBase
    {
        private readonly IHotelService _hotelService;
        private readonly IBookingService _bookingService;

        public HotelController(IHotelService hotelService, IBookingService bookingService)
        {
            _hotelService = hotelService;
            _bookingService = bookingService;
        }

        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllHotels()
        {
            var hotels = await _hotelService.GetAllHotelsAsync();
            return Ok(hotels);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddHotel([FromBody] AddHotelDto dto)
        {
            var ownerIdClaim = User.FindFirst("HotelOwnerId")?.Value;
            if (ownerIdClaim == null)
                return Unauthorized("Invalid token or missing HotelOwnerId.");

            int ownerId = int.Parse(ownerIdClaim);
            var hotel = await _hotelService.AddHotelAsync(ownerId, dto);
            return CreatedAtAction(nameof(GetHotelById), new { hotelId = hotel.Id }, hotel);
        }

        [HttpGet("by-owner")]
        public async Task<IActionResult> GetHotelsByOwner()
        {
            var ownerIdClaim = User.FindFirst("HotelOwnerId")?.Value;
            if (ownerIdClaim == null)
                return Unauthorized("Invalid token or missing HotelOwnerId.");

            int ownerId = int.Parse(ownerIdClaim);
            var hotels = await _hotelService.GetHotelsByOwnerIdAsync(ownerId);
            return Ok(hotels);
        }

        [HttpGet("{hotelId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetHotelById(int hotelId)
        {
            var hotel = await _hotelService.GetHotelByIdAsync(hotelId);
            if (hotel == null)
                return NotFound("Hotel not found.");
            return Ok(hotel);
        }

        [HttpPatch("{hotelId}")]
        public async Task<IActionResult> UpdateHotel(int hotelId, [FromBody] AddHotelDto dto)
        {
            var ownerIdClaim = User.FindFirst("hotelOwnerId")?.Value;
            if (ownerIdClaim == null)
            {
                return BadRequest("Invalid token or missing hotelOwnerId.");
            }

            int ownerId;
            if (!int.TryParse(ownerIdClaim, out ownerId))
            {
                return BadRequest("Invalid hotelOwnerId in token.");
            }

            var hotel = await _hotelService.UpdateHotelAsync(hotelId, ownerId, dto);
            if (hotel == null) return NotFound("Hotel not found or you don’t own it.");
            return Ok(hotel);
        }

        [HttpDelete("{hotelId}")]
        public async Task<IActionResult> DeleteHotel(int hotelId)
        {
            var ownerIdClaim = User.FindFirst("HotelOwnerId")?.Value;
            if (ownerIdClaim == null)
                return Unauthorized("Invalid token or missing HotelOwnerId.");

            int ownerId = int.Parse(ownerIdClaim);
            var hotel = await _hotelService.GetHotelByIdAsync(hotelId);
            if (hotel == null || hotel.HotelOwnerId != ownerId)
                return Forbid("Unauthorized to delete this hotel.");

            var success = await _hotelService.DeleteHotelAsync(hotelId);
            if (!success) return NotFound("Hotel not found.");
            return Ok("Hotel deleted successfully.");
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchHotels([FromQuery] HotelSearchRequestDto searchParams)
        {
            Console.WriteLine($"Search Term: {searchParams.SearchTerm}");
            Console.WriteLine($"CheckInDate: {searchParams.CheckInDate}, Parsed: {searchParams.GetCheckInDateAsDateTime()}");
            Console.WriteLine($"CheckOutDate: {searchParams.CheckOutDate}, Parsed: {searchParams.GetCheckOutDateAsDateTime()}");
            Console.WriteLine($"NumberOfGuests: {searchParams.NumberOfGuests}");
            Console.WriteLine($"NumberOfRooms: {searchParams.NumberOfRooms}");
            Console.WriteLine($"MaxPrice: {searchParams.MaxPrice}");

            // Validate dates
            var checkInDate = searchParams.GetCheckInDateAsDateTime();
            var checkOutDate = searchParams.GetCheckOutDateAsDateTime();

            if (checkInDate.HasValue && checkOutDate.HasValue)
            {
                if (checkOutDate <= checkInDate)
                {
                    return BadRequest("Check-out date must be after check-in date.");
                }

                var currentDate = DateTime.Today;
                if (checkInDate < currentDate)
                {
                    return BadRequest("Check-in date cannot be in the past.");
                }
            }

            if (searchParams.NumberOfGuests <= 0)
            {
                return BadRequest("Number of guests must be greater than 0.");
            }

            if (searchParams.NumberOfRooms <= 0)
            {
                return BadRequest("Number of rooms must be greater than 0.");
            }

            try
            {
                var hotels = await _hotelService.SearchHotelsAsync(searchParams);
                return Ok(hotels);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("bookings")]
        public async Task<IActionResult> GetBookingsByOwner()
        {
            try
            {
                var ownerIdClaim = User.FindFirst("HotelOwnerId")?.Value;
                if (ownerIdClaim == null)
                    return Unauthorized("Invalid token or missing HotelOwnerId.");

                int ownerId = int.Parse(ownerIdClaim);
                var hotels = await _hotelService.GetHotelsByOwnerIdAsync(ownerId);
                var hotelIds = hotels.Select(h => h.Id).ToList();
                var bookings = await _bookingService.GetBookingsByHotelIdsAsync(hotelIds);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("cities")]
        public async Task<IActionResult> GetCities([FromQuery] string? q = null)
        {
            var cities = await _hotelService.GetDistinctCitiesAsync(q);
            return Ok(cities);
        }

    }
}
