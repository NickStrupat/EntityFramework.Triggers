EntityFramework.Triggers
=======================

Add triggers to your entities with insert, update, and delete events. There are three events for each: before, after, and upon failure.

NuGet package listed on nuget.org at https://www.nuget.org/packages/EntityFramework.Triggers/

[![NuGet Status](http://img.shields.io/nuget/v/EntityFramework.Triggers.svg?style=flat)](https://www.nuget.org/packages/EntityFramework.Triggers/)

<strong>async/await supported</strong>

## Usage

To use triggers on your entities, simply have your entities inherit `ITriggerable`, and your DbContext inherit from DbContextWithTriggers. If your DbContext inheritance chain is unchangeable, see below the example code.

	public abstract class Trackable : ITriggerable {
		public DateTime Inserted { get; private set; }
		public DateTime Updated { get; private set; }

		static Trackable() {
			Triggers<Trackable>.Inserting += entry => entry.Entity.Inserted = entry.Entity.Updated = DateTime.Now;
			Triggers<Trackable>.Updating += entry => entry.Entity.Updated = DateTime.Now;
		}
	}

	public class Person : Trackable {
		public Int64 Id { get; protected set; }
		public String Name { get; set; }
	}
	public class Context : DbContextWithTriggers {
		public DbSet<Person> People { get; set; }
	}

As you may have guessed, what we're doing above is enabling automatic insert and update stamps for any entity that inherits `Trackable`. Events are raised from the base class/interfaces, up to the events specified on the entity class being used. It's just as easy to set up soft deletes (the Deleting, Updating, and Inserting events are cancellable from within a handler, logging, auditing, and more!). You can also add handlers for single instances of an entity with `person.Triggers().Inserting...`. Handlers you add to an indiviual object will only be called on that object. The handler reference is removed after your entity is GC'd.

Check out https://github.com/NickStrupat/EntityFramework.Rx for my "Reactive Extension" wrappers for even more POWER!

If you can't easily change what your DbContext inherits from (ASP.NET Identity users, for example) you can override `SaveChanges()` in your DbContext class to call the `SaveChangesWithTriggers()` extension method. For async/await functionality, override `SaveChangesAsync(CancellationToken)` to call `SaveChangesWithTriggersAsync(cancellationToken)`. Alternatively, you can call `SaveChangesWithTriggers()` directly instead of `SaveChanges()`, although that means breaking away from the usual interface provided by `DbContext`.

	class YourContext : DbContext {
		// Your usual properties

		public override Int32 SaveChanges() {
			return this.SaveChangesWithTriggers();
		}
		public override Task<Int32> SaveChangesAsync(CancellationToken cancellationToken) {
			return this.SaveChangesWithTriggersAsync(cancellationToken);
		}
	}

**`SaveChangesWithTriggers()` and `SaveChangesWithTriggersAsync()` will call base.SaveChanges internally.**

## Longer example

	using System;
	using System.Data.Entity;
	using System.Threading;
	using System.Threading.Tasks;
	using EntityFramework.Triggers;

	namespace Example {
		class Program {
			private class Person : ITriggerable {
				public Int64 Id { get; private set; }
				public DateTime InsertDateTime { get; private set; }
				public DateTime UpdateDateTime { get; private set; }
				public String FirstName { get; set; }
				public String LastName { get; set; }
				public Boolean IsDeleted { get; set; }
				public Person() {
					this.Triggers().Inserting += entry => { entry.Entity.InsertDateTime = entry.Entity.UpdateDateTime = DateTime.Now; };
					this.Triggers().Updating += entry => { entry.Entity.UpdateDateTime = DateTime.Now; };
					this.Triggers().Deleting += entry => {
						entry.Entity.IsDeleted = true;
						entry.Cancel(); // Cancels the deletion, but will persist changes with the same effects as EntityState.Modified
					};
				}
			}
			
			private class LogEntry {
				public Int64 Id { get; private set; }
				public String Message { get; set; }
			}
			
			private class Context : DbContext {
				public DbSet<Person> People { get; set; }
				public DbSet<LogEntry> Log { get; set; }

				public override Int32 SaveChanges() {
					return this.SaveChangesWithTriggers();
				}
				public override Task<Int32> SaveChangesAsync(CancellationToken cancellationToken) {
					return this.SaveChangesWithTriggersAsync(cancellationToken);
				}
			}
			
			private static void Main(string[] args) {
				var task = MainAsync(args);
				Task.WaitAll(task);
			}
			
			private static async Task MainAsync(string[] args) {
				using (var context = new Context()) {
					var nickStrupat = new Person {
						FirstName = "Nick",
						LastName = "Strupat"
					};
					nickStrupat.Triggers().Inserting += e => {
						((Context)e.Context).Log.Add(new LogEntry { Message = "Insert trigger fired for " + e.Entity.FirstName });
						Console.WriteLine("Inserting " + e.Entity.FirstName);
					};
					nickStrupat.Triggers().Updating += e => Console.WriteLine("Updating " + e.Entity.FirstName);
					nickStrupat.Triggers().Deleting += e => Console.WriteLine("Deleting " + e.Entity.FirstName);
					nickStrupat.Triggers().Inserted += e => Console.WriteLine("Inserted " + e.Entity.FirstName);
					nickStrupat.Triggers().Updated += e => Console.WriteLine("Updated " + e.Entity.FirstName);
					nickStrupat.Triggers().Deleted += e => Console.WriteLine("Deleted " + e.Entity.FirstName);

					context.People.Add(nickStrupat);
					context.SaveChanges();

					nickStrupat.FirstName = "Nicholas";
					context.SaveChanges();

					context.People.Remove(nickStrupat);
					await context.SaveChangesAsync();

					context.Database.Delete();
				}
			}
		}
	}
