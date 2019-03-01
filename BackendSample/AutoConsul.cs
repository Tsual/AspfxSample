using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consul;
using Microsoft.AspNetCore.Hosting;
using System.Threading;
using BackendSample;

namespace BackendSample
{
    internal class AutoConsul
    {
        internal static string ConsulID { get; private set; }

        internal static async Task RegistAsync(IConfiguration configuration, IServer server)
            => await NewClient(configuration).Agent.ServiceRegister(new AgentServiceRegistration()
            {
                Address = configuration["consul:Regist:HostName"],
                ID = ConsulID = configuration["consul:Regist:Name"] + "-" + Guid.NewGuid().ToString(),
                Port = int.Parse(configuration["consul:Regist:Port"]),
                Tags = new string[] { "AutoConsul", configuration["consul:Regist:Name"], "API" },
                Name = configuration["consul:Regist:Name"],
                Check = new AgentServiceCheck()
                {
                    HTTP = server.Features.Get<IServerAddressesFeature>().Addresses.ToArray()[0] + "/" + configuration["consul:Regist:HealthCheck:Path"],
                    DockerContainerID = configuration["docker:Container:ID"] != null ? configuration["docker:Container:ID"] : null,
                    Interval = new TimeSpan(0, 0, int.Parse(configuration["consul:Regist:HealthCheck:Interval"])),
                    DeregisterCriticalServiceAfter = new TimeSpan(0, int.Parse(configuration["consul:Regist:HealthCheck:Deregist"]), 0)
                }
            });

        internal static async Task DeregistAsync(IConfiguration configuration)
            => await NewClient(configuration).Agent.ServiceDeregister(ConsulID);

        internal static ConsulClient NewClient(IConfiguration configuration)
            => new ConsulClient(arg =>
            {
                arg.Address = new Uri(configuration["consul:ServerUri"]);
                if (configuration["consul:DataCenter"] != null)
                    arg.Datacenter = configuration["consul:DataCenter"];
                if (configuration["consul:Token"] != null)
                    arg.Token = configuration["consul:Token"];
            });


    }

    public enum ConsulInvokePayload
    {
        /// <summary>
        /// dns or hardware
        /// </summary>
        Direct,
        /// <summary>
        /// cache in instance
        /// </summary>
        LocalCache,
        /// <summary>
        /// only for test
        /// </summary>
        //Redis
    }

    public class ConsulCaller : IConsulCaller
    {
        public ConsulInvokePayload InvokePayload { get; }
        private Func<string, string> UriFunction;
        private Dictionary<string, List<AgentService>> ServiceMap;
        public Action<ConsulClient> ServiceMapRefresh { get; }
        private Timer ServiceMapTimer;
        private Random ServiceMapRandom;

        public ConsulCaller(IConfiguration configuration)
        {
            InvokePayload = Enum.Parse<ConsulInvokePayload>(configuration["consul:Payload"]);
            switch (InvokePayload)
            {
                case ConsulInvokePayload.Direct:
                    UriFunction = arg => arg + ".service.consul/";
                    break;
                case ConsulInvokePayload.LocalCache:
                    ServiceMapRefresh = (consul) =>
                    {
                        if (InvokePayload != ConsulInvokePayload.LocalCache) return;
                        var task = consul.Agent.Services();
                        task.Wait();
                        var rp = new Dictionary<string, List<AgentService>>();
                        foreach (var t in task.Result.Response)
                        {
                            if (!rp.ContainsKey(t.Value.Service))
                                rp.Add(t.Value.Service, new List<AgentService>() { t.Value });
                            else
                                rp[t.Value.Service].Add(t.Value);
                        }
                        ServiceMap = rp;
                    };
                    var client = AutoConsul.NewClient(configuration);
                    ServiceMapRefresh.Invoke(client);
                    ServiceMapTimer = new Timer(x =>
                    {
                        ServiceMapRefresh.Invoke(client);
                    }, null, 5000, 5000);
                    ServiceMapRandom = new Random();
                    UriFunction = arg =>
                    {
                        if (ServiceMap.ContainsKey(arg))
                        {
                            var svLsit = ServiceMap[arg];
                            var svinfo = svLsit[ServiceMapRandom.Next(svLsit.Count)];
                            return svinfo.Address + ":" + svinfo.Port + "/";
                        }
                        throw new ArgumentException("Service[" + arg + "] Not Found");
                    };
                    break;
            }
        }

        public string GetUriHead(string ServiceName) => UriFunction.Invoke(ServiceName);
    }

    public interface IConsulCaller
    {
        Action<ConsulClient> ServiceMapRefresh { get; }
        ConsulInvokePayload InvokePayload { get; }
        string GetUriHead(string ServiceName);
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AutoConsulExtension
    {
        public static IServiceCollection AddConsulCaller(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddSingleton(new ConsulCaller(configuration));
        }
    }
}

