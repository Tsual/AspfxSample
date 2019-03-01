using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSample.Data
{
    public class RedisCache : IDisposable
    {
        public static RedisCache Instance { get; } = new RedisCache();

        Dictionary<string, IConnectionMultiplexer> ConnectionMultiplexers = new Dictionary<string, IConnectionMultiplexer>();

        public IConnectionMultiplexer this[string connect_string]
        {
            get
            {
                IConnectionMultiplexer conn = null;
                if (!ConnectionMultiplexers.ContainsKey(connect_string))
                {
                    conn = ConnectionMultiplexer.Connect(connect_string);
                    ConnectionMultiplexers.Add(connect_string, conn);
                }
                else
                {
                    conn = ConnectionMultiplexers[connect_string];
                }
                if (!conn.IsConnected)
                {
                    conn = ConnectionMultiplexer.Connect(connect_string);
                    ConnectionMultiplexers[connect_string] = conn;
                }
                return conn;
            }
        }

        public void Dispose()
        {
            foreach (var t in ConnectionMultiplexers.Values)
                t.Dispose();
        }
    }
}
