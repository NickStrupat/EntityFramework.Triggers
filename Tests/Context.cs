using System;
using System.Threading;
using System.Threading.Tasks;
using EntityFramework.Triggers;

using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;

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

	//internal sealed class Configuration : DbMigrationsConfiguration<Context> {
	//	public Configuration() {
	//		AutomaticMigrationsEnabled = true;
	//		AutomaticMigrationDataLossAllowed = true;
	//	}
	//}

	public class Context : DbContext {
		public DbSet<Person> People { get; set; }
		public DbSet<Thing> Things { get; set; }

		public override Int32 SaveChanges(Boolean acceptAllChangesOnSuccess) {
			return this.SaveChangesWithTriggers(acceptAllChangesOnSuccess);
		}
#if !NET40
		public override Task<Int32> SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken)) {
			return this.SaveChangesWithTriggersAsync(acceptAllChangesOnSuccess, cancellationToken);
		}
#endif
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
			optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=Triggers.Tests;integrated security=True;");
			base.OnConfiguring(optionsBuilder);
		}
	}
}
