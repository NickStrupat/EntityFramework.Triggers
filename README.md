EntityFramework.Triggers
========================

Add triggers to your entities with insert, update, and delete events. There are three events for each: before, after, and upon failure.

This repo contains the code for both the `EntityFramework` and `EntityFrameworkCore` projects, as well as the ASP.NET Core support projects.

### Nuget packages for triggers

| EF version  | .NET support                                    | NuGet package                                                                                                                                              |
|:------------|:------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------|
| >= 6.1.3    | >= Framework 4.6.2                              | [![NuGet Status](http://img.shields.io/nuget/v/EntityFramework.Triggers.svg?style=flat)](https://www.nuget.org/packages/EntityFramework.Triggers/)         |
| >= .NET 6 | >= .NET 6 | [![NuGet Status](http://img.shields.io/nuget/v/EntityFrameworkCore.Triggers.svg?style=flat)](https://www.nuget.org/packages/EntityFrameworkCore.Triggers/) |

### Nuget packages for ASP.NET Core dependency injection methods

| EF version  | .NET support                                    | NuGet package                                                                                                                                                            |
|:------------|:------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| >= 6.1.3    | >= Framework 4.6.2                              | [![NuGet Status](http://img.shields.io/nuget/v/NickStrupat.EntityFramework.Triggers.AspNetCore.svg?style=flat)](https://www.nuget.org/packages/NickStrupat.EntityFramework.Triggers.AspNetCore/)|
| >= .NET 6 | >= .NET 6 | [![NuGet Status](http://img.shields.io/nuget/v/NickStrupat.EntityFrameworkCore.Triggers.AspNetCore.svg?style=flat)](https://www.nuget.org/packages/NickStrupat.EntityFrameworkCore.Triggers.AspNetCore/)               |

## Basic usage with a global singleton

To use triggers on your entities, simply have your DbContext inherit from `DbContextWithTriggers`. If you can't change your DbContext inheritance chain, you simply need to override your `SaveChanges...` as demonstrated [below](#manual-overriding-to-enable-triggers)

```csharp
public abstract class Trackable {
	public DateTime Inserted { get; private set; }
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

## Usage with dependency injection

This library fully supports dependency injection. The two features are:

1) Injecting the triggers and handler registrations to avoid the global singleton in previous versions

```csharp
serviceCollection
	.AddSingleton(typeof(ITriggers<,>), typeof(Triggers<,>))
	.AddSingleton(typeof(ITriggers<>), typeof(Triggers<>))
	.AddSingleton(typeof(ITriggers), typeof(Triggers));
```

2) Using injected services right inside your global handlers

```csharp
Triggers<Person, Context>().GlobalInserted.Add<IServiceBus>(
	entry => entry.Service.Broadcast("Inserted", entry.Entity)
);

Triggers<Person, Context>().GlobalInserted.Add<(IServiceBus Bus, IServiceX X)>(
	entry => {
		entry.Service.Bus.Broadcast("Inserted", entry.Entity);
		entry.Service.X.DoSomething();
	}
);
```

3) Using injected services right inside your injected handlers

```csharp
public class Startup
{
	public void ConfigureServices(IServiceCollection services)
	{
		...
		services.AddDbContext<Context>();
		services.AddTriggers();
	}

	public void Configure(IApplicationBuilder app, IHostingEnvironment env)
	{
		...
		app.UseTriggers(builder =>
		{
			builder.Triggers().Inserted.Add(
				entry => Debug.WriteLine(entry.Entity.ToString())
			);
			builder.Triggers<Person, Context>().Inserted.Add(
				entry => Debug.WriteLine(entry.Entity.FirstName)
			);

			// receive injected services inside your handler, either with just a single service type or with a value tuple of services
			builder.Triggers<Person, Context>().GlobalInserted.Add<IServiceBus>(
				entry => entry.Service.Broadcast("Inserted", entry.Entity)
			);
			builder.Triggers<Person, Context>().GlobalInserted.Add<(IServiceBus Bus, IServiceX X)>(
				entry => {
					entry.Service.Bus.Broadcast("Inserted", entry.Entity);
					entry.Service.X.DoSomething();
				}
			);
		});
	}
}
```

## How to enable triggers if you can't derive from `DbContextWithTriggers`

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

#region If you didn't/can't override `SaveChanges...`, you can (not recommended) call 
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

		public abstract class SoftDeletable : Trackable {
			public virtual DateTime? Deleted { get; private set; }

			public Boolean IsSoftDeleted => Deleted != null;
			public void SoftDelete() => Deleted = DateTime.UtcNow;
			public void SoftRestore() => Deleted = null;

			static SoftDeletable() {
				Triggers<SoftDeletable>.Deleting += entry => {
					entry.Entity.SoftDelete();
					entry.Cancel = true; // Cancels the deletion, but will persist changes with the same effects as EntityState.Modified
				};
			}
		}

		public class Person : SoftDeletable {
			public virtual Int64 Id { get; private set; }
			public virtual String FirstName { get; set; }
			public virtual String LastName { get; set; }
		}

		public class LogEntry {
			public virtual Int64 Id { get; private set; }
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
			Triggers<Person>.Updating += e => Console.WriteLine($"Updating {e.Original.FirstName} to {e.Entity.FirstName}");
			Triggers<Person>.Deleting += e => Console.WriteLine("Deleting " + e.Entity.FirstName);
			Triggers<Person>.Inserted += e => Console.WriteLine("Inserted " + e.Entity.FirstName);
			Triggers<Person>.Updated += e => Console.WriteLine("Updated " + e.Entity.FirstName);
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

- [https://github.com/NickStrupat/EntityFramework.Rx](https://github.com/NickStrupat/EntityFramework.Rx) for **hot** observables of your EF operations
- [https://github.com/NickStrupat/EntityFramework.PrimaryKey](https://github.com/NickStrupat/EntityFramework.PrimaryKey) to easily get the primary key of any entity (including composite keys)
- [https://github.com/NickStrupat/EntityFramework.TypedOriginalValues](https://github.com/NickStrupat/EntityFramework.TypedOriginalValues) to get a proxy object of the orginal values of your entity (typed access to Property("...").OriginalValue)
- [https://github.com/NickStrupat/EntityFramework.SoftDeletable](https://github.com/NickStrupat/EntityFramework.SoftDeletable) for base classes which encapsulate the soft-delete pattern (including keeping a history with user id, etc.)
- [https://github.com/NickStrupat/EntityFramework.VersionedProperties](https://github.com/NickStrupat/EntityFramework.VersionedProperties) for a library of classes which auto-magically keep an audit history of the changes to the specified property

## Contributing

1. [Create an issue](https://github.com/NickStrupat/EntityFramework.Triggers/issues/new)
2. Let's find some point of agreement on your suggestion.
3. Fork it!
4. Create your feature branch: `git checkout -b my-new-feature`
5. Commit your changes: `git commit -am 'Add some feature'`
6. Push to the branch: `git push origin my-new-feature`
7. Submit a pull request :D

## History

[Commit history](https://github.com/NickStrupat/EntityFramework.Triggers/commits/master)

## License

[MIT License](https://github.com/NickStrupat/EntityFramework.Triggers/blob/master/README.md)
