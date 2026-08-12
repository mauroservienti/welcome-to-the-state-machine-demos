using Finance.Data;
using Finance.Data.Models;
using Finance.Messages.Commands;
using NServiceBus;
using System.Drawing;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace Finance.Service.Handlers
{
    class StoreReservedTicketHandler : IHandleMessages<StoreReservedTicket>
    {
        readonly FinanceContext db;

        public StoreReservedTicketHandler(FinanceContext db)
        {
            this.db = db;
        }

        public async Task Handle(StoreReservedTicket message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Finance Adding ticket '{message.TicketId}' to reservation '{message.ReservationId}'.", Color.Green);

            db.ReservedTickets.Add(new ReservedTicket() 
            { 
                ReservationId = message.ReservationId, 
                TicketId = message.TicketId 
            });

            Console.WriteLine($"Ticket added.", Color.Green);
            await Task.CompletedTask;
        }
    }
}





// using Finance.Data;
// using Finance.Data.Models;
// using Finance.Messages.Commands;
// using NServiceBus;
// using NServiceBus.Persistence.Sql; // 🌟 ADD THIS FOR ACCESSING THE INTERNAL SQL SESSION
// using Microsoft.EntityFrameworkCore;
// using System.Drawing;
// using System.Threading.Tasks;
// using Console = Colorful.Console;

// namespace Finance.Service.Handlers
// {
//     class StoreReservedTicketHandler : IHandleMessages<StoreReservedTicket>
//     {
//         public async Task Handle(StoreReservedTicket message, IMessageHandlerContext context)
//         {
//             Console.WriteLine($"Finance Adding ticket '{message.TicketId}' to reservation '{message.ReservationId}'.", Color.Green);

//             // 🔓 1. Extract NServiceBus's internal SQL storage session for THIS message
//             var sqlSession = context.SynchronizedStorageSession.SqlPersistenceSession();

//             // 🗄️ 2. Feed NServiceBus's active connection straight into your EF DbContext
//             var optionsBuilder = new DbContextOptionsBuilder<FinanceContext>();
//             optionsBuilder.UseNpgsql(sqlSession.Connection); // Shares the physical PostgreSQL connection socket

//             await using var db = new FinanceContext(optionsBuilder.Options);

//             // 🛡️ 3. Force EF Core to attach right onto NServiceBus's current transaction boundary
//             db.Database.UseTransaction(sqlSession.Transaction);

//             // 4. Run your regular business mutations
//             db.ReservedTickets.Add(new ReservedTicket()
//             {
//                 ReservationId = message.ReservationId,
//                 TicketId = message.TicketId
//             });

//             // 5. Save changes safely inside the shared transaction block
//             await db.SaveChangesAsync(context.CancellationToken);

//             Console.WriteLine($"Ticket added.", Color.Green);
//         }
//     }
// }




// using Finance.Data;
// using Finance.Data.Models;
// using Finance.Messages.Commands;
// using NServiceBus;
// using System;
// using System.Drawing;
// using System.Threading.Tasks;
// using Console = Colorful.Console;

// namespace Finance.Service.Handlers
// {
//     class StoreReservedTicketHandler : IHandleMessages<StoreReservedTicket>
//     {
//         readonly Func<FinanceContext> contextFactory;

//         public StoreReservedTicketHandler() : this(() => new FinanceContext())
//         {
//         }

//         internal StoreReservedTicketHandler(Func<FinanceContext> contextFactory)
//         {
//             this.contextFactory = contextFactory;
//         }

//         public async Task Handle(StoreReservedTicket message, IMessageHandlerContext context)
//         {
//             Console.WriteLine($"Finance Adding ticket '{message.TicketId}' to reservation '{message.ReservationId}'.", Color.Green);

//             await using var db = contextFactory();
//             db.ReservedTickets.Add(new ReservedTicket() { ReservationId = message.ReservationId, TicketId = message.TicketId });
//             await db.SaveChangesAsync(context.CancellationToken);


//             Console.WriteLine($"Ticket added.", Color.Green);
//         }
//     }
// }
