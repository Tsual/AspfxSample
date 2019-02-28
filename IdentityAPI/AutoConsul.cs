using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consul;
using Microsoft.AspNetCore.Hosting;

namespace IdentityAPI
{
    internal class AutoConsul
    {
        internal static string ConsulID { get; private set; }

        internal static async Task RegistAsync(IConfiguration configuration, IServer server)
            =>await NewClient(configuration).Agent.ServiceRegister(new AgentServiceRegistration()
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
            =>await NewClient(configuration).Agent.ServiceDeregister(ConsulID);

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
}
