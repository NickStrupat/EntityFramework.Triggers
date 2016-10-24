using System;

#if EF_CORE
namespace EntityFrameworkCore.Triggers.Tests {
#else
namespace EntityFramework.Triggers.Tests {
#endif

	public abstract class EntityWithInsertTracking {
#if EF_CORE
		public DateTime Inserted { get; protected set; }
		public Int32 Number { get; protected set; }
#else
		public DateTime Inserted { get; private set; }
		public Int32 Number { get; private set; }
#endif

		static EntityWithInsertTracking() {
			Triggers<EntityWithInsertTracking>.Inserting += e => e.Entity.Inserted = DateTime.UtcNow;
			Triggers<EntityWithInsertTracking>.Inserting += e => e.Entity.Number = 42;
		}
	}
	public abstract class EntityWithTracking : EntityWithInsertTracking {
#if EF_CORE
		public DateTime Updated { get; protected set; }
#else
		public DateTime Updated { get; private set; }
#endif

		static EntityWithTracking() {
			Triggers<EntityWithTracking>.Inserting += e => e.Entity.Updated = DateTime.UtcNow;
			Triggers<EntityWithTracking>.Updating += e => e.Entity.Updated = DateTime.UtcNow;
		}
	}
}
