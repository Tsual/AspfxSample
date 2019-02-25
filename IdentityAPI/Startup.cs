using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityAPI.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace IdentityAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddDbContext<SqliteContext>();
            services.AddLogging();
            //services.AddDistributedRedisCache(arg =>
            //{
            //    arg.Configuration = Configuration["redis:connnect_string"];
            //    arg.InstanceName = Configuration["redis:instance_name"];
            //});
            services.AddResponseCaching();
            services.Add(new ServiceDescriptor(typeof(IConnectionMultiplexer), factory: (sp) => ConnectionMultiplexer.Connect(Configuration["redis:connect_string"]), lifetime: ServiceLifetime.Singleton));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            appLifetime.ApplicationStarted.Register(callback: () =>
            {
                WarmUp.DoWork(Configuration);
            });


            app.UseResponseCaching();
            app.UseMvc();
        }


    }
}
