using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BackendSample.Data;
using BackendSample.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using IdentityModel.Client;

namespace BackendSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly SqlitePooledContext _context;
        private readonly ILogger<IdentityController> _logger;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IConfiguration _configuration;

        public IdentityController(SqlitePooledContext context, ILogger<IdentityController> logger, IConnectionMultiplexer connectionMultiplexer, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _connectionMultiplexer = connectionMultiplexer;
            _configuration = configuration;
        }

        [HttpPost("verify")]
        public IActionResult Verify([FromBody] ModelIn model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(model.Password))
                return BadRequest();

            IQueryable<mUser> query_result = null;
            if (!string.IsNullOrWhiteSpace(model.LoginId))
                query_result = from t in _context.sUser where t.LoginId == model.LoginId select t;
            else if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
                query_result = from t in _context.sUser where t.PhoneNumber == model.PhoneNumber select t;
            else if (!string.IsNullOrWhiteSpace(model.Email))
                query_result = from t in _context.sUser where t.Email == model.Email select t;

            var query_list = query_result.Take(1).ToList();
            if (query_list.Count < 1) return NotFound();

            if (query_list[0].Password == model.Password)
                return Ok(new { Jwt = LocalJwt.Regist(_connectionMultiplexer.GetDatabase(), query_list[0].ID.ToString(), _configuration["jwt:Issuer"], int.Parse(_configuration["jwt:Overtime"])) });

            return BadRequest("User Not Found");
        }

        [HttpPost("regist")]
        public async Task<IActionResult> RegistAsync([FromBody]ModelIn model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(model.Password))
                return BadRequest("Password Empty");

            var query_result = from t in _context.sUser select t;
            if (!string.IsNullOrWhiteSpace(model.LoginId))
                query_result = query_result.Where(t => t.LoginId == model.LoginId);
            if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
                query_result = query_result.Where(t => t.PhoneNumber == model.PhoneNumber);
            if (!string.IsNullOrWhiteSpace(model.Email))
                query_result = query_result.Where(t => t.Email == model.Email);

            var query_list = query_result.Take(1).ToList();
            if (query_list.Count > 0) return BadRequest("User Already Exist");

            var user = new mUser()
            {
                Email = model.Email,
                LoginId = model.LoginId,
                Password = model.Password,
                PhoneNumber = model.PhoneNumber
            };
            _context.sUser.Attach(user);
            await _context.SaveChangesAsync();
            return Ok(new { Jwt = LocalJwt.Regist(_connectionMultiplexer.GetDatabase(), user.ID.ToString(), _configuration["jwt:Issuer"], int.Parse(_configuration["jwt:Overtime"])) });
        }

        public class ModelIn
        {
            public string Password { get; set; }

            public string Email { get; set; }

            public string LoginId { get; set; }

            public string PhoneNumber { get; set; }
        }
    }



}