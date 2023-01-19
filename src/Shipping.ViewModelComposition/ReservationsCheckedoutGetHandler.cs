using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ServiceComposer.AspNetCore;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Shipping.ViewModelComposition
{
    class ReservationsCheckedoutGetHandler : ICompositionRequestsHandler
    {
        [HttpGet("/reservations/checkedout")]
        public Task Handle(HttpRequest request)
        {
            /* 
             * delete the reservation delivery option cookie
             */
            var response = request.HttpContext.Response;
            response.Cookies.Append(
                key: "reservation-delivery-option-id",
                value: "",
                options: new CookieOptions()
                {
                    Expires = DateTimeOffset.Now.AddHours(-1)
                });

            return Task.CompletedTask;
        }
    }
}
