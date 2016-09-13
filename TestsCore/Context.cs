using System;
using System.Threading;
using System.Threading.Tasks;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
using EntityFrameworkCore.Triggers;
#else
using EntityFramework.Triggers;
using System.Data.Entity;
using System.Data.Entity.Migrations;
#endif

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

	internal sealed class Configuration : DbMigrationsConfiguration<Context> {
		public Configuration() {
			AutomaticMigrationsEnabled = true;
			AutomaticMigrationDataLossAllowed = true;
		}
	}

	public class Context : DbContext {
		public Context() : base("Triggers.Tests") { }

		public DbSet<Person> People { get; set; }
		public DbSet<Thing> Things { get; set; }

		public override Int32 SaveChanges() {
			return this.SaveChangesWithTriggers();
		}

		public override Task<Int32> SaveChangesAsync(CancellationToken cancellationToken) {
			return this.SaveChangesWithTriggersAsync(cancellationToken);
		}
	}
}
