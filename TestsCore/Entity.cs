using System;

#if EF_CORE
namespace EntityFrameworkCore.Triggers.Tests {
#else
namespace EntityFramework.Triggers.Tests {
#endif

	public abstract class EntityWithInsertTracking {
		public DateTime Inserted { get; protected set; }
		public Int32 Number { get; protected set; }

		static EntityWithInsertTracking() {
			Triggers<EntityWithInsertTracking>.Inserting += e => e.Entity.Inserted = DateTime.UtcNow;
			Triggers<EntityWithInsertTracking>.Inserting += e => e.Entity.Number = 42;
		}
	}
	public abstract class EntityWithTracking : EntityWithInsertTracking {
		public DateTime Updated { get; protected set; }

		static EntityWithTracking() {
			Triggers<EntityWithTracking>.Inserting += e => e.Entity.Updated = DateTime.UtcNow;
			Triggers<EntityWithTracking>.Updating += e => e.Entity.Updated = DateTime.UtcNow;
		}
	}
}
