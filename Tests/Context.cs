using System.Data.Entity;
using EntityFramework.Triggers;

namespace Tests {
    public class Context : DbContextWithTriggers {
		public DbSet<Person> People { get; set; }
		public DbSet<Thing> Things { get; set; }
	}
}
