using System.Data.Entity;
using EntityFrameworkTriggers;

namespace Tests {
    public class Context : DbContextWithTriggers<Context> {
        public DbSet<Person> People { get; set; }
        public DbSet<Thing> Things { get; set; }
    }
}
