using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ServiceComposer.AspNetCore;
using System;
using System.Threading.Tasks;

namespace Reservations.ViewModelComposition
{
    class ReservationsCheckedoutGetHandler : IHandleRequests
    {
        public bool Matches(RouteData routeData, string httpVerb, HttpRequest request)
        {
            var controller = (string)routeData.Values["controller"];
            var action = (string)routeData.Values["action"];

            return HttpMethods.IsGet(httpVerb)
                   && controller.ToLowerInvariant() == "reservations"
                   && action.ToLowerInvariant() == "checkedout";
        }

        public Task Handle(string requestId, dynamic vm, RouteData routeData, HttpRequest request)
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
