EntityFramework.Triggers
=======================

Add triggers to your entities with insert, update, and delete events. There are three events for each: before, after, and upon failure.

NuGet package listed on nuget.org at https://www.nuget.org/packages/EntityFramework.Triggers/

<strong>async/await supported</strong>

## Usage

To use triggers on your entities, you simply need to have your entities inherit from `ITriggerable`, and override `SaveChanges()` in your DbContext class to call the `SaveChangesWithTriggers(base.SaveChanges)` extension method. For async/await functionality, override `SaveChangesAsync(CancellationToken)` to call `SaveChangesWithTriggersAsync(base.SaveChangesAsync, cancellationToken)`.

	class YourContext : DbContext {
		// Your usual properties

		public override Int32 SaveChanges() {
			return this.SaveChangesWithTriggers(base.SaveChanges);
		}
		public override Task<Int32> SaveChangesAsync(CancellationToken cancellationToken) {
			return this.SaveChangesWithTriggersAsync(base.SaveChangesAsync, cancellationToken);
		}
	}

## Example

	using System;
	using System.Data.Entity;
	using System.Threading;
	using System.Threading.Tasks;
	using EntityFramework.Triggers;

	namespace Example {
		class Program {
			private class Person : ITriggerable {
				public Int64 Id { get; protected set; }
				public DateTime InsertDateTime { get; protected set; }
				public DateTime UpdateDateTime { get; protected set; }
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
				public Int64 Id { get; protected set; }
				public String Message { get; set; }
			}
			private class Context : DbContext {
				public DbSet<Person> People { get; set; }
				public DbSet<LogEntry> Log { get; set; }

				public override Int32 SaveChanges() {
					return this.SaveChangesWithTriggers(base.SaveChanges);
				}
				public override Task<Int32> SaveChangesAsync(CancellationToken cancellationToken) {
					return this.SaveChangesWithTriggersAsync(base.SaveChangesAsync, cancellationToken);
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