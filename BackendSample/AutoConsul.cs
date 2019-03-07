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
using Microsoft.AspNetCore.Builder;

namespace BackendSample
{
    internal class AutoConsul
    {
        internal static AgentServiceRegistration serviceRegistration { get; private set; }

        internal static async Task RegistAsync(Action<ConsulClientConfiguration> configOverride, AgentServiceRegistration registration)
        => await NewClient(configOverride).Agent.ServiceRegister(serviceRegistration = registration);

        internal static async Task DeregistAsync(Action<ConsulClientConfiguration> configOverride)
            => await NewClient(configOverride).Agent.ServiceDeregister(serviceRegistration.ID);

        internal static ConsulClient NewClient(Action<ConsulClientConfiguration> configOverride)
            => new ConsulClient(configOverride);
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
        private Action<ConsulClient> ServiceMapRefresh { get; }
        private Timer ServiceMapTimer;
        private Random ServiceMapRandom;
        private Action<ConsulClientConfiguration> configOverride;

        public ConsulCaller(Action<ConsulClientConfiguration> configOverride, ConsulInvokePayload InvokePayload)
        {
            this.InvokePayload = InvokePayload;
            this.configOverride = configOverride;
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
                    var client = AutoConsul.NewClient(configOverride);
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

        public ConsulClient NewClient() => AutoConsul.NewClient(configOverride);
    }

    public interface IConsulCaller
    {
        //Action<ConsulClient> ServiceMapRefresh { get; }
        ConsulInvokePayload InvokePayload { get; }
        string GetUriHead(string ServiceName);
        ConsulClient NewClient();
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AutoConsulExtension
    {
        public static IServiceCollection AddConsulCaller(this IServiceCollection services, Action<ConsulClientConfiguration> configOverride, ConsulInvokePayload InvokePayload)
            => services.AddSingleton(new ConsulCaller(configOverride, InvokePayload));

        public static IApplicationLifetime EnableConsul(this IApplicationLifetime applicationLifetime, Action<ConsulClientConfiguration> configOverride, AgentServiceRegistration registration)
        {
            applicationLifetime.ApplicationStarted.Register(callback: async () =>
            {
                await AutoConsul.RegistAsync(configOverride, registration);
            });

            applicationLifetime.ApplicationStopping.Register(callback: () =>
            {
                //防止主线程过快退出而导致进程退出
                AutoConsul.DeregistAsync(configOverride).Wait();
            });

            return applicationLifetime;
        }
    }
}

