﻿using Hotel_Booking_App.Models.DTOs.Payment;

namespace Hotel_Booking_App.Interface.Payment
{
    public interface IPaymentRepository
    {
        Task<PaymentResponseDto> ProcessPaymentAsync(PaymentRequestDto paymentRequest);
        Task<List<PaymentResponseDto>> GetPaidBookingsAsync();
        Task<List<PaymentResponseDto>> GetUnpaidBookingsAsync();
    }
}
