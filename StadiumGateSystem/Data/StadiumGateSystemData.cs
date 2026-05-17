using Microsoft.EntityFrameworkCore;
using StadiumGateSystem.Models;

namespace StadiumGateSystem.Data
{
    public class StadiumGateSystemDbContext : DbContext
    {
        public StadiumGateSystemDbContext(
            DbContextOptions<StadiumGateSystemDbContext> options)
            : base(options)
        {
        }

        public DbSet<GateData> Gates { get; set; }
    }
}
