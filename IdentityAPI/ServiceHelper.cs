using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityAPI
{
    public class ServiceHelper
    {
        public static List<Dictionary<string, string>> DiInfo { get; set; } = new List<Dictionary<string, string>>();

        public static string DiInfoToJson()
        {
            return JsonConvert.SerializeObject(DiInfo);
        }
    }
}
