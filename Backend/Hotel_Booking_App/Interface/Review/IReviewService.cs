using Hotel_Booking_App.Models.DTOs.Review;

namespace Hotel_Booking_App.Interface.Review
{
    public interface IReviewService
    {
        Task<ReviewResponseDto> AddReviewAsync(ReviewRequestDto request, int? customerId, string customerName);
        Task<List<ReviewResponseDto>> GetReviewsByHotelIdAsync(int hotelId);
        Task<ReviewResponseDto> UpdateReviewAsync(int reviewId, ReviewRequestDto request, int customerId);
    }
}
