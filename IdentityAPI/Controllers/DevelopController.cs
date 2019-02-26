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
        private readonly IServiceHelper serviceHelper;
        public DevelopController(ILogger<DevelopController> logger, IServiceHelper serviceHelper)
        {
            this.logger = logger;
            this.serviceHelper = serviceHelper;
        }
        [HttpGet("LogServices")]
        public IActionResult LogServices()
        {
            logger.LogDebug(serviceHelper.DiInfoToJson());
            return Ok();
        }
    }
}