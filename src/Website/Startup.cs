using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using Reservations.ViewModelComposition.Middlewares;
using ServiceComposer.AspNetCore;
using ServiceComposer.AspNetCore.Mvc;
using System;

namespace Website
{
    public class Startup
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddViewModelComposition(options =>
            {
                options.AddMvcSupport();
            });

            return services.AddNServiceBus("Webapp", config => 
            {
                config.ApplyCommonConfiguration(asSendOnly: true);
                return config.UseMicrosoftDependencyInjection(services);
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
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
