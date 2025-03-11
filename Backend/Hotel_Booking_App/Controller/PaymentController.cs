using Hotel_Booking_App.Interface.Payment;
using Hotel_Booking_App.Models.DTOs.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hotel_Booking_App.Controllers
{
    [Route("api/payments")]
    [ApiController]
    [EnableCors("AllowAllOrigins")]
    [Authorize] 
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("process-payment")] 
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestDto paymentRequest)
        {
            if (paymentRequest == null)
                return BadRequest("Invalid payment request.");

            try
            {
                var paymentResponse = await _paymentService.ProcessPaymentAsync(paymentRequest);
                return Ok(paymentResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("paid-bookings")]
        public async Task<ActionResult<List<PaymentResponseDto>>> GetPaidBookings()
        {
            try
            {
                var paidBookings = await _paymentService.GetPaidBookingsAsync();
                return Ok(paidBookings);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("unpaid-bookings")]
        public async Task<ActionResult<List<PaymentResponseDto>>> GetUnpaidBookings()
        {
            try
            {
                var unpaidBookings = await _paymentService.GetUnpaidBookingsAsync();
                return Ok(unpaidBookings);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
