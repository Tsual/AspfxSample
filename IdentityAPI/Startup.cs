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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Helper;
using IdentityAPI.Core;

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
            services.Add(new ServiceDescriptor(typeof(IConnectionMultiplexer), factory: (sp) => RedisCache.Instance[Configuration["redis:connect_string"]], lifetime: ServiceLifetime.Singleton));
            services.AddAuthentication(arg =>
            {
                arg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(arg =>
            {
                
                arg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = JwtCore.SecretKey,

                    ValidateIssuer = true,
                    ValidIssuer = Configuration["jwt:Issuer"],

                    ValidateAudience = true,
                    //ValidAudience = JwtSetting.Audience,

                    // Validate the token expiry
                    ValidateLifetime = true,

                    // If you want to allow a certain amount of clock drift, set that here:
                    ClockSkew = TimeSpan.Zero,

                    AudienceValidator = (aud, key, token) =>
                   {
                       bool res = true;
                       foreach (var aud_t in aud)
                           res &= JwtCore.Check(RedisCache.Instance[Configuration["redis:connect_string"]].GetDatabase(), aud_t);
                       return res;
                   },

                    //IssuerValidator = (iss, key, token) =>
                    //  {
                    //      return "";
                    //  },
                };
            });

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
