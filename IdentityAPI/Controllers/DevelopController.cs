using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Consul;
using Microsoft.AspNetCore.Hosting.Server;

namespace IdentityAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevController : ControllerBase
    {
        private readonly ILogger<DevController> logger;
        private readonly IServiceHelper serviceHelper;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment;
        private readonly IServer server;
        public DevController(ILogger<DevController> logger, IServiceHelper serviceHelper, Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment,IServer server)
        {
            this.logger = logger;
            this.serviceHelper = serviceHelper;
            this.hostingEnvironment = hostingEnvironment;
            this.server = server;
        }
        [HttpGet("logdi")]
        public IActionResult LogServices()
        {
            logger.LogDebug(serviceHelper.DiInfoToJson());
            return Ok();
        }
        [HttpGet("consul")]
        public async Task<IActionResult> consulAsync()
        {
            ConsulClient consulClient = new ConsulClient(cfg => {
                cfg.Address = new Uri("http://localhost:8500");
                cfg.Datacenter = "Tdx";
            });

            var agent =consulClient.Agent;
            var svs=await agent.Services();

            foreach (var t in server.Features)
                logger.LogDebug(t.ToString());

            return new ContentResult() { Content = "ops" };
        }
    }
}