EntityFramework.Triggers
=======================

Add triggers to your entities with insert, update, and delete events. There are three events for each: before, after, and upon failure.

| EF version | .NET support                          | NuGet package                                                                                                                                              |
|:-----------|:--------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 6.1.3      | == 4.0 &#124;&#124; >= 4.5            | [![NuGet Status](http://img.shields.io/nuget/v/EntityFramework.Triggers.svg?style=flat)](https://www.nuget.org/packages/EntityFramework.Triggers/)         |
| Core 1.1   | >= 4.5.1 &#124;&#124; >= Standard 1.3 | [![NuGet Status](http://img.shields.io/nuget/v/EntityFrameworkCore.Triggers.svg?style=flat)](https://www.nuget.org/packages/EntityFrameworkCore.Triggers/) |

This repo contains the code for both the `EntityFramework` and `EntityFrameworkCore` projects.

<strong>async/await supported</strong>

## Basic usage

To use triggers on your entities, simply have your DbContext inherit from `DbContextWithTriggers`. If you can't change your DbContext inheritance chain, you simply need to override your `SaveChanges...` as demonstrated [below](#manual-overriding-to-enable-triggers)

```csharp
public abstract class Trackable {
	public DateTime Inserted { get; private set; } // note that if using EF Core, these setters must be `protected` (likely a bug in EF Core)
	public DateTime Updated { get; private set; }

	static Trackable() {
		Triggers<Trackable>.Inserting += entry => entry.Entity.Inserted = entry.Entity.Updated = DateTime.UtcNow;
		Triggers<Trackable>.Updating += entry => entry.Entity.Updated = DateTime.UtcNow;
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

## Manual overriding to enable triggers

If you can't easily change what your `DbContext` class inherits from (ASP.NET Identity users, for example), you can override your `SaveChanges...` methods to call the `SaveChangesWithTriggers...` extension methods. Alternatively, you can call `SaveChangesWithTriggers...` directly instead of `SaveChanges...` if, for example, you want to control which changes cause triggers to be fired.

```csharp
class YourContext : DbContext {
	// Your usual DbSet<> properties

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

## See also

- [https://github.com/NickStrupat/EntityFramework.Rx]() for **hot** observables of your EF operations
- [https://github.com/NickStrupat/EntityFramework.PrimaryKey]() to easily get the primary key of any entity (including composite keys)
- [https://github.com/NickStrupat/EntityFramework.TypedOriginalValues]() to get a proxy object of the orginal values of your entity (typed access to Property("...").OriginalValue)
- [https://github.com/NickStrupat/EntityFramework.SoftDeletable]() for base classes which encapsulate the soft-delete pattern (including keeping a history with user id, etc.)
- [https://github.com/NickStrupat/EntityFramework.VersionedProperties]() for a library of classes which auto-magically keep an audit history of the changes to the specified property
