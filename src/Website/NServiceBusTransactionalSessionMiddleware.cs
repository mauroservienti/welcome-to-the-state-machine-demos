using Microsoft.AspNetCore.Http;
using NServiceBus.TransactionalSession;
using System.Threading.Tasks;

namespace Website
{
    public class NServiceBusTransactionalSessionMiddleware
    {
        private readonly RequestDelegate _next;

        public NServiceBusTransactionalSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITransactionalSession transactionalSession)
        {
            if (HttpMethods.IsPost(context.Request.Method))
            {
                await transactionalSession.Open(new SqlPersistenceOpenSessionOptions(), context.RequestAborted);
                try
                {
                    await _next(context);
                    await transactionalSession.Commit(context.RequestAborted);
                }
                catch
                {
                    // Don't commit if an error occurs - pending messages will not be dispatched
                    throw;
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}
