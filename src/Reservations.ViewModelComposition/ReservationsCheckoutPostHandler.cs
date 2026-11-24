using Microsoft.AspNetCore.Http;
using Heyi;
using NServiceBus;
using Reservations.Messages.Commands;
using ServiceComposer.AspNetCore;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Reservations.ViewModelComposition
{
    class ReservationsCheckoutPostHandler : ICompositionRequestsHandler
    {
        private readonly IOutgoingMessages messages;

        public ReservationsCheckoutPostHandler(IOutgoingMessages messages)
        {
            this.messages = messages;
        }

        [HttpPost("/reservations/checkout")]
        public Task Handle(HttpRequest request)
        {
            /*
             * In a production environment if multiple services are interested in the
             * same post request the handling logic is much more complex than what we
             * are doing in this demo. In this demo both Finance and Reservations need
             * to handle the POST to /reservations/checkout. The implementation assumes
             * that the host/infrastructure never fails, which is not the case in a
             * production environment. In order to make this part safe, which is not the
             * scope of this demo asynchronous messaging should be introduced earlier in
             * the processing pipeline.
             *
             * More information: https://milestone.topics.it/2019/05/02/safety-first.html
             */

            var message = new CheckoutReservation()
            {
                ReservationId = new Guid(request.Cookies["reservation-id"])
            };

            /*
             * WARN: destination is hardcoded to reduce demo complexity.
             * In a production environment routing should be configured
             * at startup by the host/infrastructure.
             */
            var options = new SendOptions();
            options.SetDestination("Reservations.Service");
            return messages.Send(message, options, request.HttpContext.RequestAborted);
        }
    }
}
