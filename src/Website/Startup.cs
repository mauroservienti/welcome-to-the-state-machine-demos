using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus.TransactionalSession;
using Reservations.ViewModelComposition.Middlewares;
using ServiceComposer.AspNetCore;

namespace Website
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddControllersWithViews();

            services.AddViewModelComposition();
            services.AddHeyi(options => options.OpenSessionOptions(() => new SqlPersistenceOpenSessionOptions()));
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseMiddleware<ReservationMiddleware>();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                // Heyi wraps each controller action and the composition handlers composing over it
                // in one transactional session: their messages are sent only if the request succeeds.
                // Composition over controllers must be the innermost endpoint filter, so it goes last.
                endpoints.MapControllers()
                    .WithTransactionalSession()
                    .WithCompositionOverControllers();
                endpoints.MapCompositionHandlers();
            });
        }
    }
}
