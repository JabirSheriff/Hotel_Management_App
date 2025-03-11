using Hotel_Booking_App.Exceptions;
using Hotel_Booking_App.Interface.Review;
using Hotel_Booking_App.Models.DTOs.Review;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hotel_Booking_App.Controllers
{
    [ApiController]
    [EnableCors("AllowAllOrigins")]
    [Route("api/reviews")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ICustomerService _customerService;

        public ReviewController(IReviewService reviewService, ICustomerService customerService)
        {
            _reviewService = reviewService;
            _customerService = customerService;
        }

        
        [HttpPost]
        [AllowAnonymous]  
        public async Task<IActionResult> AddReview([FromBody] ReviewRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int? customerId = null;
            string customerName = "Anonymous";

            
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    var customer = await _customerService.GetCustomerByUserIdAsync(userId);
                    if (customer != null)
                    {
                        customerId = customer.Id;
                        customerName = customer.FullName;
                    }
                }
            }

            var review = await _reviewService.AddReviewAsync(request, customerId, customerName);
            return Ok(review);
        }

        
        [HttpGet("{hotelId}")]
        public async Task<IActionResult> GetReviewsByHotelId(int hotelId)
        {
            var reviews = await _reviewService.GetReviewsByHotelIdAsync(hotelId);
            return Ok(reviews);
        }

        
        [HttpPut("{reviewId}")]
        [Authorize]
        public async Task<IActionResult> UpdateReview(int reviewId, [FromBody] ReviewRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid token: User ID is missing.");
            }

            var customer = await _customerService.GetCustomerByUserIdAsync(userId);
            if (customer == null)
            {
                return Unauthorized("Customer account not found.");
            }

            var updatedReview = await _reviewService.UpdateReviewAsync(reviewId, request, customer.Id);
            return Ok(updatedReview);
        }
    }
}
