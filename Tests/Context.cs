using System.Data.Entity;
using EntityFrameworkTriggers;

namespace Tests {
    public class Context : DbContextWithTriggers<Context> {
        public DbSet<Person> People { get; set; }
        public DbSet<Thing> Things { get; set; }
    }

	public class SealedContext : DbContext {
		public DbSet<TriggerablePerson> People { get; set; }
		public DbSet<TriggerableThing> Things { get; set; }
	}
}
