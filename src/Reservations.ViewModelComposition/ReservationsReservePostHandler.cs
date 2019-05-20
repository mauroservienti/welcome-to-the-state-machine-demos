using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NServiceBus;
using Reservations.Messages.Commands;
using ServiceComposer.AspNetCore;
using System;
using System.Threading.Tasks;

namespace Reservations.ViewModelComposition
{
    class ReservationsReservePostHandler : IHandleRequests
    {
        private readonly IMessageSession messageSession;

        public ReservationsReservePostHandler(IMessageSession messageSession)
        {
            this.messageSession = messageSession;
        }

        public bool Matches(RouteData routeData, string httpVerb, HttpRequest request)
        {
            var controller = (string)routeData.Values["controller"];
            var action = (string)routeData.Values["action"];

            return HttpMethods.IsPost(httpVerb)
                   && controller.ToLowerInvariant() == "reservations"
                   && action.ToLowerInvariant() == "reserve"
                   && routeData.Values.ContainsKey("id");
        }

        public Task Handle(string requestId, dynamic vm, RouteData routeData, HttpRequest request)
        {
            /*
             * In a production envronment if multiple services are interested in the
             * same post request the handling logic is much more complex than what we
             * are doing in this demo. In this demo both Finance and Reservations need
             * to handle the POST to /reservations/reserve. The implementation assumes
             * that the host/infrastructure never fails, which is not the case in a
             * production environment. In order to make this part safe, which is not the
             * scope of this demo asynchronous messaging should be introduced earlier in
             * the processing pipeline.
             * 
             * More information: https://milestone.topics.it/2019/05/02/safety-first.html
             */
            var message = new ReserveTicket()
            {
                TicketId = int.Parse((string)routeData.Values["id"]),
                ReservationId = new Guid(request.Cookies["reservation-id"])
            };

            /*
             * WARN: destination is hardcoded to reduce demo complexity.
             * In a production environment routing should be configured
             * at startup by the host/infrastructure.
             */
            return messageSession.Send("Reservations.Service", message);
        }
    }
}
