using NServiceBus;
using Reservations.Messages.Commands;
using System;
using System.Threading.Tasks;

namespace Reservations.Service.Handlers
{
    class ReserveTicketHandler : IHandleMessages<ReserveTicket>
    {
        public Task Handle(ReserveTicket message, IMessageHandlerContext context)
        {
            throw new NotImplementedException();
        }
    }
}
