using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EntityFramework.Triggers;

namespace Example {
	class Program {
		public abstract class Trackable : ITriggerable {
			public virtual DateTime InsertDateTime { get; protected set; }
			public virtual DateTime UpdateDateTime { get; protected set; }

			protected Trackable() {
				this.Triggers().Inserting += entry => { entry.Entity.InsertDateTime = entry.Entity.UpdateDateTime = DateTime.Now; };
				this.Triggers().Updating += entry => { entry.Entity.UpdateDateTime = DateTime.Now; };
			}
		}

		public class WhatPerson : Person
		{
			private DbPropertyValues dbpv;

			public WhatPerson(DbPropertyValues dbpv)
			{
				this.dbpv = dbpv;
			}

			public override string FirstName
			{
				get { return dbpv.GetValue<String>(nameof(FirstName)); }
				set { /*base.FirstName = value;*/ }
			}
		}

		public class Person : Trackable {
			public virtual Int64 Id { get; protected set; }
			public virtual String FirstName { get; set; }
			public virtual String LastName { get; set; }
			public virtual Boolean IsDeleted { get; set; }

			static Person() {
				Triggers<Person>.Deleting += entry => {
					entry.Entity.IsDeleted = true;
					entry.Cancel(); // Cancels the deletion, but will persist changes with the same effects as EntityState.Modified
				};
			}
		}
		public class LogEntry {
			public virtual Int64 Id { get; protected set; }
			public virtual String Message { get; set; }
		}
		public class Context : DbContext {
			public virtual DbSet<Person> People { get; set; }
			public virtual DbSet<LogEntry> Log { get; set; }

			public override Int32 SaveChanges() {
				return this.SaveChangesWithTriggers();
			}
			public override Task<Int32> SaveChangesAsync(CancellationToken cancellationToken) {
				return this.SaveChangesWithTriggersAsync(cancellationToken);
			}
		}
		internal sealed class Configuration : DbMigrationsConfiguration<Context> {
			public Configuration() {
				AutomaticMigrationsEnabled = true;
			}
		}
		private static void Main(string[] args) {
		//	var task = MainAsync(args);
		//	Task.WaitAll(task);
		//}
		//private static async Task MainAsync(string[] args) {
			using (var context = new Context()) {
				var log = context.Log.ToList();
				var nickStrupat = new Person {
					FirstName = "Nick",
					LastName = "Strupat"
				};
				nickStrupat.Triggers().Inserting += e => {
					((Context)e.Context).Log.Add(new LogEntry { Message = "Insert trigger fired for " + e.Entity.FirstName });
					Console.WriteLine("Inserting " + e.Entity.FirstName);
				};
				nickStrupat.Triggers().Updating += e => Console.WriteLine("Updating " + e.Original.FirstName);
				nickStrupat.Triggers().Deleting += e => Console.WriteLine("Deleting " + e.Entity.FirstName);
				nickStrupat.Triggers().Inserted += e => Console.WriteLine("Inserted " + e.Entity.FirstName);
				nickStrupat.Triggers().Updated += e => Console.WriteLine("Updated " + e.Entity.FirstName);
				nickStrupat.Triggers().Deleted += e => Console.WriteLine("Deleted " + e.Entity.FirstName);

				context.People.Add(nickStrupat);
				context.SaveChanges();

				nickStrupat.FirstName = "Nicholas";
				context.SaveChanges();

				context.People.Remove(nickStrupat);
				//await context.SaveChangesAsync();
				context.SaveChanges();
			}
		}
	}
}