using Microsoft.EntityFrameworkCore;
using ObjectEntity = StajP.Entities.Object;

namespace StajP.Data
{
    public class ObjectDbContext : DbContext
    {
        public DbSet<ObjectEntity> Objects { get; set; }

        public ObjectDbContext(DbContextOptions<ObjectDbContext> options)
            : base(options)
        {
        }
    }
}