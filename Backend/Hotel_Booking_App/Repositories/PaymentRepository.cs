using Hotel_Booking_App.Contexts;
using Hotel_Booking_App.Interface.Payment;
using Hotel_Booking_App.Models;
using Hotel_Booking_App.Models.DTOs.Payment;
using HotelBookingApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel_Booking_App.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly HotelBookingDbContext _context;

        public PaymentRepository(HotelBookingDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentResponseDto> ProcessPaymentAsync(PaymentRequestDto paymentRequest)
        {
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
                .FirstOrDefaultAsync(b => b.Id == paymentRequest.BookingId);

            if (booking == null)
                throw new KeyNotFoundException("Booking not found.");

            if (booking.Status == BookingStatus.Paid)
                throw new InvalidOperationException("Booking is already paid.");

            
            decimal totalAmount = booking.BookingRooms.Sum(br => br.Room.PricePerNight) * (booking.CheckOutDate - booking.CheckInDate).Days;

            var payment = new Payment
            {
                BookingId = booking.Id,
                CustomerId = booking.CustomerId ?? 0,
                Amount = totalAmount,
                PaymentMethod = paymentRequest.PaymentMethod,
                Status = PaymentStatus.Completed,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            
            booking.Status = BookingStatus.Paid;
            await _context.SaveChangesAsync();

            return new PaymentResponseDto
            {
                PaymentId = payment.Id,
                BookingId = booking.Id,
                AmountPaid = payment.Amount,
                PaymentMethod = payment.PaymentMethod.ToString(),
                PaymentDate = payment.CreatedAt,
                PaymentStatus = payment.Status.ToString(),
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                RoomPricePerNight = booking.BookingRooms.First().Room.PricePerNight
            };
        }

        public async Task<List<PaymentResponseDto>> GetPaidBookingsAsync()
        {
            return await _context.Payments
                .Include(p => p.Booking)
                .ThenInclude(b => b.Customer)
                .Include(p => p.Booking)
                .ThenInclude(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
                .Where(p => p.Status == PaymentStatus.Completed)
                .Select(p => new PaymentResponseDto
                {
                    PaymentId = p.Id,
                    BookingId = p.Booking.Id,
                    AmountPaid = p.Amount,
                    PaymentMethod = p.PaymentMethod.ToString(),
                    PaymentDate = p.CreatedAt,
                    PaymentStatus = p.Status.ToString(),
                    CheckInDate = p.Booking.CheckInDate,
                    CheckOutDate = p.Booking.CheckOutDate,
                    RoomPricePerNight = p.Booking.BookingRooms.First().Room.PricePerNight
                })
                .ToListAsync();
        }

        public async Task<List<PaymentResponseDto>> GetUnpaidBookingsAsync()
        {
            return await _context.Bookings
                .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
                .Where(b => !_context.Payments.Any(p => p.BookingId == b.Id && p.Status == PaymentStatus.Completed))
                .Select(b => new PaymentResponseDto
                {
                    BookingId = b.Id,
                    AmountPaid = 0,
                    PaymentMethod = "Not Paid",
                    PaymentDate = null, 
                    PaymentStatus = "Unpaid",
                    CheckInDate = b.CheckInDate, 
                    CheckOutDate = b.CheckOutDate, 
                    RoomPricePerNight = b.BookingRooms.FirstOrDefault() != null
                        ? b.BookingRooms.FirstOrDefault().Room.PricePerNight
                        : 0 
                })
                .ToListAsync();
        }


    }
}
