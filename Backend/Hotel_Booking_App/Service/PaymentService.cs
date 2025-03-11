//using Hotel_Booking_App.Interface.Bookings;
using Hotel_Booking_App.Interface.Payment;
using Hotel_Booking_App.Models;
using Hotel_Booking_App.Models.DTOs.Payment;
using HotelBookingApp.Interfaces;
using HotelBookingApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel_Booking_App.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IBookingRepository _bookingRepository;

        public PaymentService(IPaymentRepository paymentRepository, IBookingRepository bookingRepository)
        {
            _paymentRepository = paymentRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<PaymentResponseDto> ProcessPaymentAsync(PaymentRequestDto paymentDto)
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(paymentDto.BookingId);
            if (booking == null)
                throw new Exception("Booking not found.");

            if (booking.Status == BookingStatus.Paid)
                throw new Exception("Booking is already completed.");

            decimal totalAmount = booking.BookingRooms.Sum(br => br.Room.PricePerNight) * (booking.CheckOutDate - booking.CheckInDate).Days;

            var payment = new Payment
            {
                BookingId = booking.Id,
                CustomerId = booking.CustomerId ?? 0,
                Amount = totalAmount,
                PaymentMethod = paymentDto.PaymentMethod,
                Status = PaymentStatus.Completed, 
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var paymentResponse = await _paymentRepository.ProcessPaymentAsync(paymentDto);
            booking.Status = BookingStatus.Paid;
            await _bookingRepository.UpdateBookingAsync(booking);

            return paymentResponse;
        }

        public async Task<List<PaymentResponseDto>> GetPaidBookingsAsync()
        {
            return await _paymentRepository.GetPaidBookingsAsync();
        }

        public async Task<List<PaymentResponseDto>> GetUnpaidBookingsAsync()
        {
            return await _paymentRepository.GetUnpaidBookingsAsync();
        }
    }
}
