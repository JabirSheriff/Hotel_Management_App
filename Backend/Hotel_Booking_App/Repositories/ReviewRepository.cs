using Hotel_Booking_App.Contexts;
using Hotel_Booking_App.Exceptions;
using Hotel_Booking_App.Interface.Review;
using Hotel_Booking_App.Models;
using Hotel_Booking_App.Models.DTOs.Review;
using HotelBookingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Booking_App.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly HotelBookingDbContext _context;

        public ReviewRepository(HotelBookingDbContext context)
        {
            _context = context;
        }

        public async Task<ReviewResponseDto> AddReviewAsync(ReviewRequestDto reviewRequest, int? customerId, string customerName)
        {
            var review = new Review
            {
                CustomerId = customerId, 
                HotelId = reviewRequest.HotelId,
                Rating = reviewRequest.Rating,
                Comment = reviewRequest.Comment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return new ReviewResponseDto
            {
                ReviewId = review.Id,
                HotelId = review.HotelId,
                CustomerName = customerId.HasValue ? customerName : "Anonymous",
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
        }

        public async Task<List<ReviewResponseDto>> GetReviewsByHotelIdAsync(int hotelId)
        {
            return await _context.Reviews
                .Where(r => r.HotelId == hotelId)
                .Include(r => r.Customer)
                .Select(r => new ReviewResponseDto
                {
                    ReviewId = r.Id,
                    HotelId = r.HotelId,
                    CustomerName = r.CustomerId != 0 ? r.Customer.FullName : "Anonymous",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<ReviewResponseDto> UpdateReviewAsync(int reviewId, ReviewRequestDto request, int customerId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null || review.CustomerId != customerId)
            {
                throw new UnauthorizedAccessException("You can only edit your own reviews.");
            }

            review.Rating = request.Rating;
            review.Comment = request.Comment;
            review.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ReviewResponseDto
            {
                ReviewId = review.Id,
                HotelId = review.HotelId,
                CustomerName = review.Customer.FullName,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
        }
    }
}
