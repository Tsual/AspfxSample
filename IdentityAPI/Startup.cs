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
using Newtonsoft.Json;

namespace IdentityAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env, ILogger<Startup> logger)
        {
            Configuration = configuration;
            HostingEnvironment = env;
            Logger = logger;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }
        public ILogger<Startup> Logger { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        /* framework di service
            Microsoft.AspNetCore.Hosting.Builder.IApplicationBuilderFactory	暂时
            Microsoft.AspNetCore.Hosting.IApplicationLifetime	单例
            Microsoft.AspNetCore.Hosting.IHostingEnvironment	单例
            Microsoft.AspNetCore.Hosting.IStartup	单例
            Microsoft.AspNetCore.Hosting.IStartupFilter	暂时
            Microsoft.AspNetCore.Hosting.Server.IServer	单例
            Microsoft.AspNetCore.Http.IHttpContextFactory	暂时
            Microsoft.Extensions.Logging.ILogger<T>	单例
            Microsoft.Extensions.Logging.ILoggerFactory	单例
            Microsoft.Extensions.ObjectPool.ObjectPoolProvider	单例
            Microsoft.Extensions.Options.IConfigureOptions<T>	暂时
            Microsoft.Extensions.Options.IOptions<T>	单例
            System.Diagnostics.DiagnosticSource	单例
            System.Diagnostics.DiagnosticListener	单例 */
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddDbContext<SqliteContext>();
            services.AddLogging(arg=> {
                arg.AddConsole();
                if (HostingEnvironment.IsDevelopment())
                    arg.AddProvider(new DiskLogProvider());
            });
            services.AddDistributedRedisCache(arg =>
            {
                arg.Configuration = Configuration["redis:connnect_string"];
                arg.InstanceName = Configuration["redis:instance_name"];
            });
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
            services.AddCap(arg =>
            {
                arg.UseRabbitMQ(cfg =>
                {
                    cfg.HostName = Configuration["rabbitmq:HostName"];
                });
                arg.UseDashboard(cfg =>
                {

                });
                arg.UseDiscovery(cfg =>
                {
                    cfg.DiscoveryServerHostName = Configuration["consul:ServerHostName"];
                    cfg.DiscoveryServerPort = int.Parse(Configuration["consul:ServerPort"]);
                    cfg.CurrentNodeHostName = Configuration["consul:CurrentNodeHostName"];
                    cfg.CurrentNodePort = int.Parse(Configuration["consul:CurrentNodePort"]);
                    cfg.NodeId = int.Parse(Configuration["consul:NodeId"]);
                    cfg.NodeName = Configuration["consul:NodeName"];
                });
                arg.UseMySql(Configuration["mysql:cap:connect_string"]);
            });

            if (HostingEnvironment.IsDevelopment())
            {
                foreach (var sv in services)
                    ServiceHelper.DiInfo.Add(new Dictionary<string, string>() {
                        {"Factory",sv.ImplementationFactory?.ToString() },
                        {"ImplIns",sv.ImplementationInstance?.ToString() },
                        {"ImplType",sv.ImplementationType?.ToString() },
                        {"Isv",sv.ServiceType?.ToString() },
                        {"LifeTime",sv.Lifetime.ToString() },
                    });
                Logger.LogDebug("Final DI Container:" + ServiceHelper.DiInfoToJson());
            }
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


