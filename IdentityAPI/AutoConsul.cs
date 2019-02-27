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
    public class AutoConsul
    {
        public static void Regist(IConfiguration configuration,IApplicationLifetime lifetime) {
            ConsulClient client = new ConsulClient(arg =>
              {
                  arg.Address = new Uri(configuration["consul:ServerUri"]);
                  if (configuration["consul:DataCenter"] != null)
                      arg.Datacenter = configuration["consul:DataCenter"];
                  if (configuration["consul:Token"] != null)
                      arg.Token = configuration["consul:Token"];
              });

            //client.Agent.ServiceRegister

        }
    }
}
