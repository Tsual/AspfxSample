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
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;
using System.Text;
using RabbitMQ.Client.Events;
using System.Net;
using System.Reflection;

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
        private readonly ObjectPoolProvider objectPoolProvider;
        private readonly ConnectionFactory rabbitFactory;
        public DevController(
            ILogger<DevController> logger,
            IServiceHelper serviceHelper,
            Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment,
            IServer server,
            ObjectPoolProvider objectPoolProvider,
            ConnectionFactory rabbitFactory)
        {
            this.logger = logger;
            this.serviceHelper = serviceHelper;
            this.hostingEnvironment = hostingEnvironment;
            this.server = server;
            this.objectPoolProvider = objectPoolProvider;
            this.rabbitFactory = rabbitFactory;
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
            ConsulClient consulClient = new ConsulClient(cfg =>
            {
                cfg.Address = new Uri("http://localhost:8500");
                //cfg.Datacenter = "Tdx";
            });

            var agent = consulClient.Agent;
            var svs = await agent.Services();

            foreach (var t in server.Features)
                logger.LogDebug(t.ToString());

            var appDomain=AppDomain.CurrentDomain.GetAssemblies();
           

            return new ContentResult() { Content = "ops" };
        }
        [HttpPost("mqsend")]
        public IActionResult rabbitsend(string msg)
        {
            using (var conn = rabbitFactory.CreateConnection())
            using (var channel = conn.CreateModel())
            {
                channel.QueueDeclare(queue: "dev",
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);
                channel.BasicPublish("", "dev", null, Encoding.UTF8.GetBytes(msg));
            }
            return Ok();
        }
        [HttpGet("mqreceive")]
        public IActionResult rabbitreceive()
        {
            using (var conn = rabbitFactory.CreateConnection())
            using (var channel = conn.CreateModel())
            {
                var rec = channel.BasicGet("dev", true);
                return Ok(rec!=null?Encoding.UTF8.GetString(rec.Body):"null");
            }
        }
    }
}