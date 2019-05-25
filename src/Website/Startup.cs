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
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

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

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseMiddleware<ReservationMiddleware>();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
