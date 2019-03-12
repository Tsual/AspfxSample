using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendSample.Controllers
{
    //在2.2中使用https://www.nuget.org/packages/Microsoft.AspNetCore.Diagnostics.HealthChecks 中的healthcheck
    //[Route("[controller]")]
    //[ApiController]
    //public class HealthController : ControllerBase
    //{
    //    [HttpGet]
    //    [ResponseCache(Duration = int.MaxValue)]
    //    public IActionResult HeartBeat()
    //    {
    //        return Ok();
    //    }
    //}

    [Route("[controller]")]
    [ApiController]
    public class ErrorController : ControllerBase
    {
        [HttpGet]
        [ResponseCache(Duration = int.MaxValue)]
        public IActionResult HeartBeat()
        {
            return BadRequest();
        }
    }
}