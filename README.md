EntityFramework.Triggers
=======================

Add triggers to your entities with insert, update, and delete events. There are three events for each: before, after, and upon failure.

**This is the branch with EF7 support**

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

As you may have guessed, what we're doing above is enabling automatic insert and update stamps for any entity that inherits `Trackable`. Events are raised from the base class/interfaces, up to the events specified on the entity class being used. It's just as easy to set up soft deletes (the Deleting, Updating, and Inserting events are cancellable from within a handler, logging, auditing, and more! You can also add handlers for single instances of an entity with `person.Triggers().Inserting...`. Handlers you add to an indiviual object will only be called on that object. The handler reference is removed after your entity is GC'd.

Check out https://github.com/NickStrupat/EntityFramework.Rx for my "Reactive Extension" wrappers for even more POWER!

If you can't easily change what your DbContext inherits from (ASP.NET Identity users, for example) you can override `SaveChanges()` in your DbContext class to call the `SaveChangesWithTriggers()` extension method. For async/await functionality, override `SaveChangesAsync(CancellationToken)` to call `SaveChangesWithTriggersAsync(cancellationToken)`. Alternatively, you can call `SaveChangesWithTriggers()` directly instead of `SaveChanges()`, although that means breaking away from the usual interface provided by `DbContext`.

	class YourContext : DbContext {
		// Your usual properties

		public override Int32 SaveChanges(Boolean acceptAllChangesOnSuccess) {
			return this.SaveChangesWithTriggers(acceptAllChangesOnSuccess);
		}
		public override Task<Int32> SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken)) {
			return this.SaveChangesWithTriggersAsync(acceptAllChangesOnSuccess, cancellationToken);
		}
	}

**`SaveChangesWithTriggers()` and `SaveChangesWithTriggersAsync()` will call base.SaveChanges internally.**
