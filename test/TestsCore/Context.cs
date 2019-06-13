using System;

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
        private static String GetConnectionString(String databaseName) => $@"Server=(localdb)\mssqllocaldb;Database={databaseName};Trusted_Connection=True;";

#if !EF_CORE
        public Context(IServiceProvider serviceProvider) : base(serviceProvider, GetConnectionString(typeof(Context).FullName)) {}
#endif
#if EF_CORE
        public Context(IServiceProvider serviceProvider) : base(serviceProvider) { }
        public Context(IServiceProvider serviceProvider, DbContextOptions options) : base(serviceProvider, options) {}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
			optionsBuilder.UseSqlServer(GetConnectionString(GetType().FullName));
		}
#endif

		public DbSet<Person>    People     { get; set; }
		public DbSet<Thing>     Things     { get; set; }
		public DbSet<Apple>     Apples     { get; set; }
		public DbSet<RoyalGala> RoyalGalas { get; set; }
	}
}
