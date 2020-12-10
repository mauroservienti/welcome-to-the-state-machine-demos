using Microsoft.AspNetCore.Http;
using ServiceComposer.AspNetCore;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Reservations.ViewModelComposition
{
    class ReservationsCheckedoutGetHandler : ICompositionRequestsHandler
    {
        [HttpGet("/reservations/checkedout")]
        public Task Handle(HttpRequest request)
        {
            /*
             * delete the reservation cookie so to let
             * infrastructure create a new reservation
             */
            var response = request.HttpContext.Response;
            response.Cookies.Append(
                key: "reservation-id",
                value: "",
                options: new CookieOptions()
                {
                    Expires = DateTimeOffset.Now.AddHours(-1)
                });

            return Task.CompletedTask;
        }
    }
}
