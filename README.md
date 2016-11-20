EntityFramework.Triggers
=======================

Add triggers to your entities with insert, update, and delete events. There are three events for each: before, after, and upon failure.

This repo contains the code for both the `EntityFramework` (.NET 4.0 and .NET 4.5) and `EntityFrameworkCore` (.NET 4.5.1 and .NET Standard 1.3) versions.

NuGet package listed on nuget.org at https://www.nuget.org/packages/EntityFramework.Triggers/ and https://www.nuget.org/packages/EntityFrameworkCore.Triggers/

Triggers for Entity Framework 6 (.NET 4.0 && >= .NET 4.5) [![NuGet Status](http://img.shields.io/nuget/v/EntityFramework.Triggers.svg?style=flat)](https://www.nuget.org/packages/EntityFramework.Triggers/)

Triggers for Entity Framework Core (>= .NET 4.5.1 && >= .NET Standard 1.3) [![NuGet Status](http://img.shields.io/nuget/v/EntityFrameworkCore.Triggers.svg?style=flat)](https://www.nuget.org/packages/EntityFrameworkCore.Triggers/)

<strong>async/await supported</strong>

## Usage

To use triggers on your entities, simply have your DbContext inherit from DbContextWithTriggers. If your DbContext inheritance chain is unchangeable, see below the example code.

```csharp
	public abstract class Trackable {
		public DateTime Inserted { get; private set; }
		public DateTime Updated { get; private set; }

		static Trackable() {
			Triggers<Trackable>.Inserting += entry => entry.Entity.Inserted = entry.Entity.Updated = DateTime.Now;
			Triggers<Trackable>.Updating += entry => entry.Entity.Updated = DateTime.Now;
		}
	}

	public class Person : Trackable {
		public Int64 Id { get; private set; }
		public String Name { get; set; }
	}

	public class Context : DbContextWithTriggers {
		public DbSet<Person> People { get; set; }
	}
```

As you may have guessed, what we're doing above is enabling automatic insert and update stamps for any entity that inherits `Trackable`. Events are raised from the base class/interfaces, up to the events specified on the entity class being used. It's just as easy to set up soft deletes (the Deleting, Updating, and Inserting events are cancellable from within a handler, logging, auditing, and more!).

Check out https://github.com/NickStrupat/EntityFramework.Rx for my "Reactive Extension" wrappers for even more POWER!

If you can't easily change what your DbContext inherits from (ASP.NET Identity users, for example) you can override `SaveChanges()` in your DbContext class to call the `SaveChangesWithTriggers()` extension method. For async/await functionality, override `SaveChangesAsync(CancellationToken)` to call `SaveChangesWithTriggersAsync(cancellationToken)`. Alternatively, you can call `SaveChangesWithTriggers()` directly instead of `SaveChanges()`, although that means breaking away from the usual interface provided by `DbContext`.

```csharp
	class YourContext : DbContext {
		// Your usual properties

		#region If you're targeting EF 6
		public override Int32 SaveChanges() {
			return this.SaveChangesWithTriggers(base.SaveChanges);
		}
		public override Task<Int32> SaveChangesAsync(CancellationToken cancellationToken) {
			return this.SaveChangesWithTriggersAsync(base.SaveChangesAsync, cancellationToken);
		}
		#endregion

		#region If you're targeting EF Core
		public override Int32 SaveChanges() {
			return this.SaveChangesWithTriggers(base.SaveChanges, acceptAllChangesOnSuccess: true);
		}
		public override Int32 SaveChanges(Boolean acceptAllChangesOnSuccess) {
			return this.SaveChangesWithTriggers(base.SaveChanges, acceptAllChangesOnSuccess);
		}
		public override Task<Int32> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken)) {
			return this.SaveChangesWithTriggersAsync(base.SaveChangesAsync, acceptAllChangesOnSuccess: true, cancellationToken: cancellationToken);
		}
		public override Task<Int32> SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken)) {
			return this.SaveChangesWithTriggersAsync(base.SaveChangesAsync, acceptAllChangesOnSuccess, cancellationToken);
		}
		#endregion
	}

	#region If you're calling `SaveChangesWithTriggers...` directly (instead of an overridden `SaveChanges...`)
	dbContext.SaveChangesWithTriggers(dbContext.SaveChanges);
	dbContext.SaveChangesWithTriggersAsync(dbContext.SaveChangesAsync);
	#endregion
```

## Longer example (targeting EF6 for now)

```csharp
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EntityFramework.Triggers;

namespace Example {
	public class Program {
		public abstract class Trackable {
			public virtual DateTime Inserted { get; private set; }
			public virtual DateTime Updated { get; private set; }

			static Trackable() {
				Triggers<Trackable>.Inserting += entry => entry.Entity.Inserted = entry.Entity.Updated = DateTime.UtcNow;
				Triggers<Trackable>.Updating += entry => entry.Entity.Updated = DateTime.UtcNow;
			}
		}

		public class Person : Trackable {
			public virtual Int64 Id { get; protected set; }
			public virtual String FirstName { get; set; }
			public virtual String LastName { get; set; }
			public virtual DateTime? Deleted { get; set; }
			public Boolean IsDeleted => Deleted != null;

			static Person() {
				Triggers<Person>.Deleting += entry => {
					entry.Entity.Deleted = DateTime.UtcNow;
					entry.Cancel(); // Cancels the deletion, but will persist changes with the same effects as EntityState.Modified
				};
			}
		}

		public class LogEntry {
			public virtual Int64 Id { get; protected set; }
			public virtual String Message { get; set; }
		}

		public class Context : DbContextWithTriggers {
			public virtual DbSet<Person> People { get; set; }
			public virtual DbSet<LogEntry> Log { get; set; }
		}
		internal sealed class Configuration : DbMigrationsConfiguration<Context> {
			public Configuration() {
				AutomaticMigrationsEnabled = true;
			}
		}

		static Program() {
			Triggers<Person, Context>.Inserting += e => {
				e.Context.Log.Add(new LogEntry { Message = "Insert trigger fired for " + e.Entity.FirstName });
				Console.WriteLine("Inserting " + e.Entity.FirstName);
			};
			Triggers<Person>.Updating += e => Console.WriteLine("Updating " + e.Entity.FirstName);
			Triggers<Person>.Deleting += e => Console.WriteLine("Deleting " + e.Entity.FirstName);
			Triggers<Person>.Inserted += e => Console.WriteLine("Inserted " + e.Entity.FirstName);
			Triggers<Person>.Updated += e => Console.WriteLine("Updated " + e.Original.FirstName);
			Triggers<Person>.Deleted += e => Console.WriteLine("Deleted " + e.Entity.FirstName);
		}
		
		private static void Main(String[] args) => Task.WaitAll(MainAsync(args));

		private static async Task MainAsync(String[] args) {
			using (var context = new Context()) {
				context.Database.Delete();
				context.Database.Create();

				var log = context.Log.ToList();
				var nickStrupat = new Person {
					FirstName = "Nick",
					LastName = "Strupat"
				};

				context.People.Add(nickStrupat);
				await context.SaveChangesAsync();

				nickStrupat.FirstName = "Nicholas";
				context.SaveChanges();
				context.People.Remove(nickStrupat);
				await context.SaveChangesAsync();
			}
		}
	}
}
```