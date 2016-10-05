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
			//optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EntityFrameworkCore.Triggers.Tests;Trusted_Connection=True;");
			//optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
			optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=EntityFramework.Triggers.Tests.Context;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
		}
#endif

		public DbSet<Person>    People     { get; set; }
		public DbSet<Thing>     Things     { get; set; }
		public DbSet<Apple>     Apples     { get; set; }
		public DbSet<RoyalGala> RoyalGalas { get; set; }

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
