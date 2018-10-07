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

	public class Context : DbContextWithTriggers {
		public Context(IServiceProvider serviceProvider) : base(serviceProvider) {}
#if EF_CORE
		public Context(IServiceProvider serviceProvider, DbContextOptions options) : base(serviceProvider, options) {}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
			optionsBuilder.UseSqlServer($@"Server=(localdb)\mssqllocaldb;Database={GetType().FullName};Trusted_Connection=True;");
		}
#endif

		public DbSet<Person>    People     { get; set; }
		public DbSet<Thing>     Things     { get; set; }
		public DbSet<Apple>     Apples     { get; set; }
		public DbSet<RoyalGala> RoyalGalas { get; set; }
	}
}
