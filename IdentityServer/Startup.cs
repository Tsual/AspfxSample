using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IdentityServer
{
    public class Startup
    {
        IConfiguration configuration;
        IHostingEnvironment env;
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            this.configuration = configuration;
            this.env = env;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(arg =>
            {
                arg.AddConsole();
                if (env.IsDevelopment())
                    arg.AddDiskLogger();
            });

            var identityBuilder = services.AddIdentityServer()
                                        .AddInMemoryIdentityResources(Config.GetIdentityResources())
                                        .AddInMemoryApiResources(Config.GetApis())
                                        .AddInMemoryClients(Config.GetClients());

            if (env.IsDevelopment())
            {
                identityBuilder.AddDeveloperSigningCredential();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseIdentityServer();
        }
    }
}
