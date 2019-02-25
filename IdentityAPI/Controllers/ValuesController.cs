using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IdentityAPI.Data;
using IdentityAPI.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using Helper;

namespace IdentityAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly SqliteContext _context;
        private readonly ILogger<ValuesController> _logger;

        public ValuesController(SqliteContext context, ILogger<ValuesController> logger)
        {
            _context = context;
            _context.Database.EnsureCreated();
            _logger = logger;
        }

        // GET: api/Values
        [HttpGet]
        public IEnumerable<mValue> GetsValue()
        {
            _logger.LogTrace("GET<<VALUE<<TAKE20");
            return _context.sValue.Take(20);
        }

        [HttpGet("init")]
        public async Task<IActionResult> InitAsync()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogTrace("GET<<INIT");
            var value_count = _context.sValue.Count();
            if (value_count < 1000)
            {
                for (int i = 1000 - value_count; i > 0; i--)
                {
                    string key = RandomString.ef16(16);
                    while (_context.sValue.Find(key) != null) key = RandomString.ef16(16);
                    await _context.sValue.AddAsync(new mValue() { Key = key, Value = RandomString.ef16(64) });
                }
                _context.SaveChangesAsync();
            }
            return Ok();
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
            var mValue = await _context.sValue.FindAsync(id);

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
                await _context.SaveChangesAsync();
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
            await _context.SaveChangesAsync();

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
            await _context.SaveChangesAsync();

            return Ok(mValue);
        }

        private bool mValueExists(string id)
        {
            return _context.sValue.Any(e => e.Key == id);
        }
    }
}