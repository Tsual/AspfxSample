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
    [Route("api/[controller]")]
    [ApiController]
    public class DevController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<DevController> logger;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment;
        private readonly IServer server;
        private readonly ObjectPoolProvider objectPoolProvider;
        private readonly ConnectionFactory rabbitFactory;
        public DevController(
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
        [HttpGet("logdi")]
        public IActionResult LogServices()
        {
            logger.LogDebug(ServiceHelper.Instance.DiInfoToJson());
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

            var appDomain = AppDomain.CurrentDomain.GetAssemblies();


            return new ContentResult() { Content = "ops" };
        }

        [HttpGet("IdentityServer4Jwt")]
        public async Task<IActionResult> ids4jwtAsync()
        {
            using (var httpClient = new HttpClient())
            {
                logger.LogTrace("IdentityServer4-Jwtoken-GetStart!");
                var disco = await httpClient.GetDiscoveryDocumentAsync(configuration["identity_server_4:url"]);
                if (disco.IsError)
                {
                    logger.LogError("IdentityServer4-Jwtoken-IdentityServerDown...");
                    return BadRequest("IdentityServer Down");
                }
                var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest()
                {
                    Address = disco.TokenEndpoint,
                    ClientId = configuration["identity_server_4:client:id"],
                    ClientSecret = configuration["identity_server_4:client:secret"],
                    Scope = configuration["identity_server_4:jwt_api"]
                });
                if (tokenResponse.IsError)
                {
                    logger.LogError("IdentityServer4-Jwtoken-TokenRequestFail...");
                    return BadRequest("TokenResponseError");
                }
                logger.LogTrace("IdentityServer4-Jwtoken-Get!<<{0}", tokenResponse.Json);
                return Ok(tokenResponse.Json);
            }
        }

    }
}