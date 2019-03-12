using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSample.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Consul;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BackendSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env, ILogger<Startup> logger, Microsoft.AspNetCore.Hosting.Server.IServer server
            , IApplicationLifetime appLifetime
            )
        {
            Configuration = configuration;
            HostingEnvironment = env;
            Logger = logger;
            this.server = server;
            this.appLifetime = appLifetime;
        }

        public Microsoft.AspNetCore.Hosting.Server.IServer server { get; }
        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }
        public ILogger<Startup> Logger { get; }
        public IApplicationLifetime appLifetime { get; }

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
            services.AddDbContextPool<SqlitePooledContext>(optionsBuilder =>
            {
                optionsBuilder.UseSqlite(Configuration["sqlite:connect_string"])
                //客户端求值时引起警告
                .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));
            });
            services.AddHttpClient();
            services.AddLogging(arg =>
            {
                arg.AddConsole();
                if (HostingEnvironment.IsDevelopment())
                    arg.AddDiskLogger();
            });
            services.AddDistributedRedisCache(arg =>
            {
                arg.Configuration = Configuration["redis:connnect_string"];
                arg.InstanceName = Configuration["redis:instance_name"];
            });
            services.AddResponseCaching();
            //可以写成静态扩展方法，这样更清楚点
            services.AddSingleton(RedisCache.Instance[Configuration["redis:connect_string"]]);
            services.AddAuthentication(arg =>
            {
                arg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            //.AddLocalJwt(Configuration)
            .AddIdentityServerJwt(Configuration)
            ;
            /** Nuget:DotNetCore.CAP
                这个包现在还没有足够完整
            services.AddCap(arg =>
            {
                arg.UseRabbitMQ(cfg =>
                {
                    cfg.HostName = Configuration["rabbitmq:HostName"];
                });
                //arg.UseDashboard();
                //dashboard 有bug
                //响应缓存更好一些
                //抽象的很好但还是需要一些客户端功能
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
            **/
            //静态扩展示例
            //非复杂情况下真的很臃肿
            services.AddRabbitMQ(arg => arg.HostName = Configuration["rabbitmq:HostName"]);
            services.AddConsulCaller(arg => arg.Address = new Uri(Configuration["consul:ServerUri"]), Enum.Parse<ConsulInvokePayload>(Configuration["consul:Payload"]));

            //sdk2.2
            services.AddHealthChecks();



            if (HostingEnvironment.IsDevelopment())
            {
                ServiceHelper.Instance.Push(services);
                //Logger.LogDebug("Final DI Container:" + ServiceHelper.Instance.DiInfoToJson());
            }

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            ConfigPipeLine(app);
            ConfigLifeTime(app);
        }

        /*
         * 内置中间件:https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/index?view=aspnetcore-2.2#built-in-middleware
        */
        private void ConfigPipeLine(IApplicationBuilder app)
        {
            ServiceHelper.ServiceProvider = app.ApplicationServices;

            if (HostingEnvironment.IsDevelopment())
            {
                // When the app runs in the Development environment:
                //   Use the Developer Exception Page to report app runtime errors.
                //   Use the Database Error Page to report database runtime errors.
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                // When the app doesn't run in the Development environment:
                //   Enable the Exception Handler Middleware to catch exceptions
                //     thrown in the following middlewares.
                //   Use the HTTP Strict Transport Security Protocol (HSTS)
                //     Middleware.
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            //sdk2.2
            app.UseHealthChecks("/health");


            //管道分叉咯
            //app.Map(route0, app0);
            //app.Map(route1, app1);

            // 如果配置了https，不过建议配在nginx上，集群内部用http就好
            // Use HTTPS Redirection Middleware to redirect HTTP requests to HTTPS.
            //app.UseHttpsRedirection();

            // Return static files and end the pipeline.
            app.UseStaticFiles();

            // Use Cookie Policy Middleware to conform to EU General Data 
            // Protection Regulation (GDPR) regulations.
            app.UseCookiePolicy();

            // 如果是razor或者mvc应用，开会话和身份验证
            //app.UseAuthentication();
            //app.UseSession();

            //对health和error进行缓存
            app.UseResponseCaching();
            app.UseMvc();
            
        }

        private void ConfigLifeTime(IApplicationBuilder app)
        {
            app.ApplicationServices.GetService<IApplicationLifetime>().UseWarmup(Configuration)
                .EnableConsul(arg =>
                {
                    arg.Address = new Uri(Configuration["consul:ServerUri"]);
                    if (Configuration["consul:DataCenter"] != null)
                        arg.Datacenter = Configuration["consul:DataCenter"];
                    if (Configuration["consul:Token"] != null)
                        arg.Token = Configuration["consul:Token"];
                }, new AgentServiceRegistration()
                {
                    Address = Configuration["consul:Regist:HostName"],
                    ID = Configuration["consul:Regist:Name"] + "-" + Guid.NewGuid().ToString(),
                    Port = int.Parse(Configuration["consul:Regist:Port"]),
                    Tags = new string[] { "AutoConsul", Configuration["consul:Regist:Name"], "API" },
                    Name = Configuration["consul:Regist:Name"],
                    Check = new AgentServiceCheck()
                    {
                        HTTP = server.Features.Get<IServerAddressesFeature>().Addresses.ToArray()[0] + "/" + Configuration["consul:Regist:HealthCheck:Path"],
                        DockerContainerID = Configuration["docker:Container:ID"] != null ? Configuration["docker:Container:ID"] : null,
                        Interval = new TimeSpan(0, 0, int.Parse(Configuration["consul:Regist:HealthCheck:Interval"])),
                        DeregisterCriticalServiceAfter = new TimeSpan(0, int.Parse(Configuration["consul:Regist:HealthCheck:Deregist"]), 0)
                    }
                })
                ;
        }

    }
}


