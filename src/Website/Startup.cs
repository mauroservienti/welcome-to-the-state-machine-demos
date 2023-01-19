using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
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

            services.AddViewModelComposition(options =>
            {
                options.EnableCompositionOverControllers(true);
                options.EnableWriteSupport();
            });
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
                endpoints.MapControllers();
                endpoints.MapCompositionHandlers();
            });
        }
    }
}
