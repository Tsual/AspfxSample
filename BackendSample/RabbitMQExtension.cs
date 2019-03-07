using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RabbitMQExtension
    {
        public static IServiceCollection AddRabbitMQ(this IServiceCollection services,Action<ConnectionFactory> FactoryConfiguration)
        {
            var conn_fac = new ConnectionFactory();
            FactoryConfiguration?.Invoke(conn_fac);
            return services.AddSingleton(conn_fac);
        }
    }
}
