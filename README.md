EntityFramework.Triggers
=======================

Adds events to your entities; inserting, inserted, updating, updated, deleting, and deleted.

NuGet package listed on nuget.org at https://www.nuget.org/packages/EntityFrameworkTriggers/

<strong>async/await supported</strong>

<strong>To use with closed inheritance hierarchies, such as ASP.NET Identity's `IdentityDbContext` class, I've introduced a set of extension methods which work without having to inherit from `DbContextWithTriggers<>` and `EntityWithTriggers<>`. Usage example for this case is at the bottom.</strong>

This version targets the latest .NET and Entity Framework versions.

For .NET 4.0 and EF5, check out https://github.com/NickStrupat/EntityFrameworkCodeFirstTriggers (NuGet link in that repository's README)

## Usage

    class Program {
		private class Person : EntityWithTriggers<Person, Context> {
			public Int64 Id { get; protected set; }
			public DateTime InsertDateTime { get; protected set; }
			public DateTime UpdateDateTime { get; protected set; }
			public String FirstName { get; set; }
			public String LastName { get; set; }
			public Person() {
				Inserting += entry => entry.Entity.InsertDateTime = entry.Entity.UpdateDateTime = DateTime.Now;
				Updating += entry => entry.Entity.UpdateDateTime = DateTime.Now;
			}
		}
		private class LogEntry {
			public Int64 Id { get; protected set; }
			public String Message { get; set; }
		}
		private class Context : DbContextWithTriggers<Context> {
			public DbSet<Person> People { get; set; }
			public DbSet<LogEntry> Log { get; set; }
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
				nickStrupat.Inserting += e => {
					e.Context.Log.Add(new LogEntry { Message = "Insert trigger fired for " + e.Entity.FirstName });
					Console.WriteLine("Inserting " + e.Entity.FirstName);
				};
				nickStrupat.Updating += e => Console.WriteLine("Updating " + e.Entity.FirstName);
				nickStrupat.Deleting += e => Console.WriteLine("Deleting " + e.Entity.FirstName);
				nickStrupat.Inserted += e => Console.WriteLine("Inserted " + e.Entity.FirstName);
				nickStrupat.Updated += e => Console.WriteLine("Updated " + e.Entity.FirstName);
				nickStrupat.Deleted += e => Console.WriteLine("Deleted " + e.Entity.FirstName);

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

## Extension method usage

	class Program {
		private class Person : ITriggerable<Person> {
			public Int64 Id { get; protected set; }
			public DateTime InsertDateTime { get; protected set; }
			public DateTime UpdateDateTime { get; protected set; }
			public String FirstName { get; set; }
			public String LastName { get; set; }
			public Person() {
				this.Triggers().Inserting += entry => { entry.Entity.InsertDateTime = entry.Entity.UpdateDateTime = DateTime.Now; };
				this.Triggers().Updating += entry => { entry.Entity.UpdateDateTime = DateTime.Now; };
			}
		}
		private class LogEntry {
			public Int64 Id { get; protected set; }
			public String Message { get; set; }
		}
		private class Context : DbContext {
			public DbSet<Person> People { get; set; }
			public DbSet<LogEntry> Log { get; set; }
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
					e.GetContext<Context>().Log.Add(new LogEntry { Message = "Insert trigger fired for " + e.Entity.FirstName });
					Console.WriteLine("Inserting " + e.Entity.FirstName);
				};
				nickStrupat.Triggers().Updating += e => Console.WriteLine("Updating " + e.Entity.FirstName);
				nickStrupat.Triggers().Deleting += e => Console.WriteLine("Deleting " + e.Entity.FirstName);
				nickStrupat.Triggers().Inserted += e => Console.WriteLine("Inserted " + e.Entity.FirstName);
				nickStrupat.Triggers().Updated += e => Console.WriteLine("Updated " + e.Entity.FirstName);
				nickStrupat.Triggers().Deleted += e => Console.WriteLine("Deleted " + e.Entity.FirstName);

				context.People.Add(nickStrupat);
				context.SaveChangesWithTriggers();

				nickStrupat.FirstName = "Nicholas";
				context.SaveChangesWithTriggers();

				context.People.Remove(nickStrupat);
				await context.SaveChangesWithTriggersAsync();

				context.Database.Delete();
			}
		}
	}
