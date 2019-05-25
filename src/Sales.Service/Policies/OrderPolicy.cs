using NServiceBus;
using Sales.Messages.Events;
using System;
using System.Threading.Tasks;
using System.Drawing;
using Console = Colorful.Console;

namespace Sales.Service.Policies
{
    class OrderPolicy : Saga<OrderPolicy.State>,
        IAmStartedByMessages<IOrderCreated>
    {
        public class State : ContainSagaData
        {
            public Guid OrderId { get; set; }
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<State> mapper)
        {
            mapper.ConfigureMapping<IOrderCreated>(m => m.OrderId).ToSaga(s => s.OrderId);
        }

        public Task Handle(IOrderCreated message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Order policy for order '{message.OrderId}' started.", Color.Green);
            /*
             * We have really nothing to do here we just
             * keep this alive for the life-cycle of the
             * order, this is it.
             */
            Data.OrderId = message.OrderId;

            return Task.CompletedTask;
        }
    }
}
