using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityAPI.Data;
using IdentityAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IdentityAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly SqliteContext _context;
        private readonly ILogger<IdentityController> _logger;

        public IdentityController(SqliteContext context, ILogger<IdentityController> logger)
        {
            _context = context;
            _context.Database.EnsureCreated();
            _logger = logger;
        }

        [HttpPost("verify")]
        public IActionResult Verify([FromBody] IdentityModel model)
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

            if (query_list[0].Password == model.Password) return Ok();

            return BadRequest();
        }

        [HttpPost("regist")]
        public async Task<IActionResult> RegistAsync([FromBody]IdentityModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(model.Password))
                return BadRequest();

            var query_result = from t in _context.sUser select t;
            if (!string.IsNullOrWhiteSpace(model.LoginId))
                query_result = query_result.Where(t => t.LoginId == model.LoginId);
            if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
                query_result = query_result.Where(t => t.PhoneNumber == model.PhoneNumber);
            if (!string.IsNullOrWhiteSpace(model.Email))
                query_result = query_result.Where(t => t.Email == model.Email);

            var query_list = query_result.Take(1).ToList();
            if (query_list.Count > 0) return BadRequest();

            _context.Entry(new mUser()
            {
                Email = model.Email,
                LoginId = model.LoginId,
                Password = model.Password,
                PhoneNumber = model.PhoneNumber
            }).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }

    public class IdentityModel
    {
        public string Password { get; set; }

        public string Email { get; set; }

        public string LoginId { get; set; }

        public string PhoneNumber { get; set; }
    }

}