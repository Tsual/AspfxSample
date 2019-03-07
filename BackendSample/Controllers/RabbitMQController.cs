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
using System.Net.Http;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using System.Threading;

namespace BackendSample.Controllers
{
    [Route("api/rab")]
    [ApiController]
    public class RabbitMQController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<DevController> logger;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment;
        private readonly IServer server;
        private readonly ObjectPoolProvider objectPoolProvider;
        private readonly ConnectionFactory rabbitFactory;
        public RabbitMQController(
            ILogger<DevController> logger,
            Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment,
            IServer server,
            ObjectPoolProvider objectPoolProvider,
            IConfiguration configuration
            , ConnectionFactory rabbitFactory
            )
        {
            this.logger = logger;
            this.hostingEnvironment = hostingEnvironment;
            this.server = server;
            this.objectPoolProvider = objectPoolProvider;
            this.configuration = configuration;
            this.rabbitFactory = rabbitFactory;
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
                return Ok(rec != null ? Encoding.UTF8.GetString(rec.Body) : "null");
            }
        }
        [HttpGet("produce")]
        public IActionResult addMqProducer()
        {
            using (var conn = rabbitFactory.CreateConnection())
            using (var model = conn.CreateModel())
            {

                model.QueueDeclare("testing", true, false, false, null);
                var publishProperties = model.CreateBasicProperties();
                publishProperties.Persistent = true;
                Guid guid = Guid.NewGuid();
                var timer = new Timer(callback =>
                {
                    model.BasicPublish("", "testing", publishProperties, Encoding.UTF8.GetBytes(DateTime.Now.ToString() + "{" + guid.ToString() + "}"));
                }, null, 100, 100);
                Thread.Sleep(5000);
                timer.Dispose();
            }
            return Ok();
        }

        [HttpGet("produce_ex")]
        public IActionResult ProduceExchange()
        {
            using (var conn = rabbitFactory.CreateConnection())
            using (var model = conn.CreateModel())
            {
                model.ExchangeDeclare("ex_test", "fanout");
                var publishProperties = model.CreateBasicProperties();
                publishProperties.Persistent = true;
                Guid guid = Guid.NewGuid();
                var timer = new Timer(callback =>
                {
                    model.BasicPublish("ex_test", "", publishProperties, Encoding.UTF8.GetBytes(DateTime.Now.ToString() + "{" + guid.ToString() + "}"));
                }, null, 100, 100);
                Thread.Sleep(5000);
                timer.Dispose();
            }
            return Ok();
        }




    }
}