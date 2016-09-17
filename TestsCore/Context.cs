using System;
using System.Threading;
using System.Threading.Tasks;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
using EntityFrameworkCore.Triggers;
namespace EntityFrameworkCore.Triggers.Tests {
#else
using EntityFramework.Triggers;
using System.Data.Entity;
using System.Data.Entity.Migrations;
namespace EntityFramework.Triggers.Tests {
#endif

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
#if EF_CORE
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
			optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EntityFrameworkCore.Triggers.Tests;Trusted_Connection=True;");
		}
#endif

		public DbSet<Person> People { get; set; }
		public DbSet<Thing> Things { get; set; }

		public override Int32 SaveChanges() {
			return this.SaveChangesWithTriggers();
		}
#if !NET40
		public override Task<Int32> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken)) {
			return this.SaveChangesWithTriggersAsync(cancellationToken);
		}
#endif
	}
}
