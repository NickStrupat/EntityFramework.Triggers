using System.Data.Entity;
using EntityFrameworkTriggers;

namespace Tests {
    public class Context : DbContextWithTriggers {
        public DbSet<Person> People { get; set; }
    }
}
