using System;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using EntityFramework.Triggers;

namespace Tests {
    public class Context : DbContextWithTriggers<Context> {
        public DbSet<Person> People { get; set; }
        public DbSet<Thing> Things { get; set; }
    }

	public class SealedContext : DbContext {
		public DbSet<TriggerablePerson> People { get; set; }
		public DbSet<TriggerableThing> Things { get; set; }

		public override Int32 SaveChanges() {
			return this.SaveChangesWithTriggers(base.SaveChanges);
		}
		public override Task<Int32> SaveChangesAsync(CancellationToken cancellationToken) {
			return this.SaveChangesWithTriggersAsync(base.SaveChangesAsync, cancellationToken);
		}
	}
}
