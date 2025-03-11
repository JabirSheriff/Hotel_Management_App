using Hotel_Booking_App.Models.DTOs;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_Booking_App.Misc
{
    [CustomExceptionFilter]
    public class CustomExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            context.Result = new BadRequestObjectResult(new ErrorObjectDto { ErrorNumber = 500, Message = context.Exception.Message });

        }
    }
}
