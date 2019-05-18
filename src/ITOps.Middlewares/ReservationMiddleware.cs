using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace ITOps.Middlewares
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
                context.Response.Cookies.Append("reservation-id", Guid.NewGuid().ToString());
            }

            await _next(context);
        }
    }
}
