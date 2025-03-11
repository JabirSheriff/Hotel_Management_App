using Hotel_Booking_App.Interface.Review;
using Hotel_Booking_App.Models;
using Hotel_Booking_App.Models.DTOs.Review;
using HotelBookingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Booking_App.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;

        public ReviewService(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<ReviewResponseDto> AddReviewAsync(ReviewRequestDto request, int? customerId, string customerName)
        {
            return await _reviewRepository.AddReviewAsync(request, customerId, customerName);
        }

        public async Task<List<ReviewResponseDto>> GetReviewsByHotelIdAsync(int hotelId)
        {
            return await _reviewRepository.GetReviewsByHotelIdAsync(hotelId);
        }

        public async Task<ReviewResponseDto> UpdateReviewAsync(int reviewId, ReviewRequestDto request, int customerId)
        {
            return await _reviewRepository.UpdateReviewAsync(reviewId, request, customerId);
        }
    }
}
