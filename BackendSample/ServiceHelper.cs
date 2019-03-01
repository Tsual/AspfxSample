using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSample
{
    internal class ServiceHelper:IServiceHelper
    {
        private ServiceHelper()
        {

        }

        public static ServiceHelper Instance { get; } = new ServiceHelper();

        public List<Dictionary<string, string>> DiInfo { get; } = new List<Dictionary<string, string>>();

        public void Push(IServiceCollection services)
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

    public interface IServiceHelper
    {
        List<Dictionary<string, string>> DiInfo { get; }
        void Push(IServiceCollection services);
        string DiInfoToJson();
    }
}
