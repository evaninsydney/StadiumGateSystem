using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StadiumGateSystem.Data;
using StadiumGateSystem.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace StadiumGateSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GateDataController : ControllerBase
    {
        private readonly StadiumGateSystemDbContext _context;
        public GateDataController(StadiumGateSystemDbContext context)
        {
            _context = context;
        }

        // GET: api/<GateDataController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GateData>>> Get()
        {
            return await _context.Gates.ToListAsync();
        }

        // GET api/<GateDataController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GateData>> Get(int id)
        {
            var gateData = await _context.Gates.FindAsync(id);

            if (gateData == null)
            {
                return NotFound();
            }

            return gateData;
        }

        // POST api/<GateDataController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GateData gateData)
        {
            if (gateData == null)
            {
                return BadRequest();
            }

            _context.Gates.Add(gateData);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = gateData.Id }, gateData);
        }

        // PUT api/<GateDataController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<GateDataController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
