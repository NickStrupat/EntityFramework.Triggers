using System;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using EntityFramework.Triggers;

namespace Tests {
    public class InternalContext : DbContext {
//        public override Int32 SaveChanges() {
//            return base.SaveChanges();
//        }
//#if !NET40
//        public override Task<Int32> SaveChangesAsync(CancellationToken cancellationToken) {
//            return base.SaveChangesAsync(cancellationToken);
//        }
//#endif
    }

    public class Context : InternalContext {
		public DbSet<Person> People { get; set; }
		public DbSet<Thing> Things { get; set; }

        public override Int32 SaveChanges() {
            return this.SaveChangesWithTriggers();
        }
#if !NET40
        public override Task<Int32> SaveChangesAsync(CancellationToken cancellationToken) {
            return this.SaveChangesWithTriggersAsync(cancellationToken);
        }
#endif
    }
}
