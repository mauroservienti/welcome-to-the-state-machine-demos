using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using ServiceComposer.AspNetCore;
using ServiceComposer.AspNetCore.Mvc;
using ITOps.UIComposition.Mvc;

namespace Website
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddUIComposition()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddViewModelComposition(options =>
            {
                options.AddMvcSupport();
            });

            return services.AddNServiceBus("Webapp")
                .ApplyCommonConfiguration(asSendOnly: true)
                .UseMicrosoftDependencyInjection();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
