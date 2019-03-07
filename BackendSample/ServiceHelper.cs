using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSample
{
    public class ServiceHelper
    {
        private ServiceHelper()
        {

        }

        public static ServiceHelper Instance { get; } = new ServiceHelper();
        public static IServiceProvider ServiceProvider { get;internal set; }

        public List<Dictionary<string, string>> DiInfo { get; } = new List<Dictionary<string, string>>();

        internal void Push(IServiceCollection services)
        {
            if (services == null) return;
            foreach (var sv in services)
                DiInfo.Add(new Dictionary<string, string>() {
                        {"Factory",sv.ImplementationFactory?.ToString() },
                        {"ImplIns",sv.ImplementationInstance?.ToString() },
                        {"ImplType",sv.ImplementationType?.ToString() },
                        {"Isv",sv.ServiceType?.ToString() },
                        {"LifeTime",sv.Lifetime.ToString() },
                    });
        }

        public string DiInfoToJson()
        {
            return JsonConvert.SerializeObject(DiInfo);
        }
    }
}
