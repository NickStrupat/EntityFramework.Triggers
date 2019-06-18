using System;
using System.Threading;
using System.Threading.Tasks;
using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;

namespace TestingAsync
{
	internal class Context : DbContextWithTriggers
	{
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (optionsBuilder.IsConfigured)
				return;
			optionsBuilder.UseSqlServer($@"Server=(localdb)\mssqllocaldb;Database={GetType().FullName};Trusted_Connection=True;ConnectRetryCount=0");
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Address>();
		}
	}

	internal class Address
	{
		public Int64 Id { get; private set; }
		public String Street { get; set; }
		public String City { get; set; }
		public Province Province { get; set; }
		public String PostalCode { get; set; }

		static Address() => Triggers<Address>.GlobalInserting.Add(entry => Task.Delay(5_000));
	}

	internal enum Province
	{
		Alberta,
		BritishColumbia,
		Manitoba,
		NewBrunswick,
		NewfoundlandAndLabrador,
		NovaScotia,
		Ontario,
		PrinceEdwardIsland,
		Quebec,
		Saskatchewan,
		NorthwestTerritories,
		Nunavut,
		Yukon,
	}

	internal class Program
	{
		private static class AppDomainCancellation
		{
			private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
			public static readonly CancellationToken Token = CancellationTokenSource.Token;
			static AppDomainCancellation() => AppDomain.CurrentDomain.ProcessExit += (_, __) => { CancellationTokenSource.Cancel(); CancellationTokenSource.Dispose(); };
		}

		private static async Task Main(String[] args)
		{
			using (var context = new Context())
			{
				await context.Database.EnsureDeletedAsync(AppDomainCancellation.Token);
				await context.Database.EnsureCreatedAsync(AppDomainCancellation.Token);
				context.Add(new Address { Street = "123 Fake St.", City = "Toronto", Province = Province.Ontario, PostalCode = "H0H0H0" });
				await context.SaveChangesAsync(AppDomainCancellation.Token);
				context.Add(new Address { Street = "456 Fake St.", City = "Vancouver", Province = Province.BritishColumbia, PostalCode = "H0H0H0" });
				context.SaveChanges();
			}
		}
	}

}
