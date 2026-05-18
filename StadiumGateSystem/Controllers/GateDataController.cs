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

        // GET api/<GateDataController>/5
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<SearchResult>>> Search(string? gate, string? type, DateTime? startDateTime, DateTime? endDateTime)
        {
            //1) get all the gate data from the database
            var gateData = await _context.Gates.ToListAsync();
            if (gateData.Count == 0) return new List<SearchResult>();

            //2)  then filter it on the search criteria
            if (!string.IsNullOrEmpty(gate))
            {
                gateData = gateData.Where(g => g.Gate.Equals(gate, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (!string.IsNullOrEmpty(type))
            {
                gateData = gateData.Where(g => g.Type.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (startDateTime.HasValue)
            {
                gateData = gateData.Where(g => g.Timestamp >= startDateTime.Value).ToList();
            }
            if (endDateTime.HasValue)
            {
                gateData = gateData.Where(g => g.Timestamp <= endDateTime.Value).ToList();
            }

            // return an empty array as there is no error, just no records
            if (!gateData.Any())
            {
                return new List<SearchResult>();
            }
            //3) Group the gate and type records and sum the number of people
            List<SearchResult> searchResults =
            [
                .. gateData.GroupBy(g => new { g.Gate, g.Type })
                    .Select(g => new SearchResult
                    {
                        Gate = g.Key.Gate,
                        Type = g.Key.Type,
                        NumberOfPeople = g.Sum(x => x.NumberOfPeople)
                    }),
            ];

            return Ok(searchResults);
        }

        // POST api/<GateDataController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GateData gateData)
        {
            if (gateData == null)
            {
                return BadRequest();
            }

            try
            {
                _context.Entry(gateData).State = EntityState.Added;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while adding the gate data.");
            }
            finally
            {
                _context.Entry(gateData).State = EntityState.Detached;
            }

            return CreatedAtAction(nameof(Get), new { id = gateData.Id }, gateData);
        }

        // PUT api/<GateDataController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] GateData gateData)
        {
            if (id != gateData.Id)
            {
                return BadRequest();
            }

            _context.Entry(gateData).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!GateDataExists(id))
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while updating the gate data.");
            }
            finally
            {
                _context.Entry(gateData).State = EntityState.Detached;
            }

            return Ok();
        }

        private bool GateDataExists(int id)
        {
            return _context.Gates.Any(e => e.Id == id);
        }

        // DELETE api/<GateDataController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            int result = 0;
            try
            {
                _context.Entry(new GateData { Id = id }).State = EntityState.Deleted;
                result = await _context.SaveChangesAsync();
                // NotFound() Nothing deleted
                // NoContent() Success (204)
                return result > 0 ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deleting the gate data.");
            }
        }
    }
}
