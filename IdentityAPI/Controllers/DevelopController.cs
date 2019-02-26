using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IdentityAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevelopController : ControllerBase
    {
        private readonly ILogger<DevelopController> logger;
        public DevelopController(ILogger<DevelopController> logger)
        {
            this.logger = logger;
        }
        [HttpGet]
        public IActionResult DevTest()
        {
            logger.LogDebug(ServiceHelper.DiInfoToJson());
            return Ok();
        }
    }
}