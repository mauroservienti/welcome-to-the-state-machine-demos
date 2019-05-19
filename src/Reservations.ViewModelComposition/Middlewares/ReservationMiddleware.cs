using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Reservations.ViewModelComposition.Middlewares
{
    public class ReservationMiddleware
    {
        private readonly RequestDelegate _next;

        public ReservationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Cookies.ContainsKey("reservation-id"))
            {
                context.Response.Cookies.Append(
                    key: "reservation-id",
                    value: Guid.NewGuid().ToString(),
                    options: new CookieOptions()
                    {
                        Expires = DateTimeOffset.Now.AddHours(1)
                    });
            }

            await _next(context);
        }
    }
}
