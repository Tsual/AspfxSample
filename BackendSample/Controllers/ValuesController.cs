using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendSample.Data;
using BackendSample.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using Helper;
using StackExchange.Redis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace BackendSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ValuesController : ControllerBase
    {
        private readonly SqliteContext _context;
        private readonly ILogger<ValuesController> _logger;
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public ValuesController(SqliteContext context, ILogger<ValuesController> logger, IConnectionMultiplexer connectionMultiplexer)
        {
            _context = context;
            _context.Database.EnsureCreated();
            _logger = logger;
            _connectionMultiplexer = connectionMultiplexer;
        }

        // GET: api/Values
        [HttpGet]
        public IEnumerable<mValue> GetsValue()
        {
            _logger.LogTrace("GET<<VALUE<<TAKE20");
            return _context.sValue.Take(20);
        }

        // GET: api/Values/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetmValue([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogTrace("GET<<KEY<<" + id);
            var mValue = (await _connectionMultiplexer.GetDatabase().StringGetAsync("VALUE_" + id)).ToString();

            if (mValue == null)
            {
                return NotFound();
            }

            return Ok(mValue);
        }

        // PUT: api/Values/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutmValue([FromRoute] string id, [FromBody] mValue mValue)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogTrace("PUT<<KEY<<" + id + "<<VALUE" + mValue.Value);

            if (id != mValue.Key)
            {
                return BadRequest();
            }

            _context.Entry(mValue).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync().ContinueWith((a) => _connectionMultiplexer.GetDatabase().StringSetAsync(mValue.RedisKey, mValue.Value));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!mValueExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Values
        [HttpPost]
        public async Task<IActionResult> PostmValue([FromBody] mValue mValue)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogTrace("POST<<" + mValue.Key + "VALUE<<" + mValue.Value);
            _context.sValue.Add(mValue);
            await _context.SaveChangesAsync().ContinueWith(t => _connectionMultiplexer.GetDatabase().StringSetAsync(mValue.RedisKey, mValue.Value));

            return CreatedAtAction("GetmValue", new { id = mValue.Key }, mValue);
        }

        // DELETE: api/Values/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletemValue([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogTrace("DELETE<<KEY<<" + id);
            var mValue = await _context.sValue.FindAsync(id);
            if (mValue == null)
            {
                return NotFound();
            }

            _context.sValue.Remove(mValue);
            await _context.SaveChangesAsync().ContinueWith(t => _connectionMultiplexer.GetDatabase().StringDecrement(mValue.RedisKey));

            return Ok(mValue);
        }

        private bool mValueExists(string id)
        {
            return _context.sValue.Any(e => e.Key == id);
        }
    }
}