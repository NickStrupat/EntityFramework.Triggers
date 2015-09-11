using System;
using EntityFramework.Triggers;

namespace Tests {
	public abstract class EntityWithInsertTracking : ITriggerable {
		public DateTime Inserted { get; private set; }
		public Int32 Number { get; private set; }
		static EntityWithInsertTracking() {
			Triggers<EntityWithInsertTracking>.Inserting += e => e.Entity.Inserted = DateTime.UtcNow;
			Triggers<EntityWithInsertTracking>.Inserting += e => e.Entity.Number = 42;
		}
	}
	public abstract class EntityWithTracking : EntityWithInsertTracking {
		public DateTime Updated { get; private set; }
		protected EntityWithTracking() {
			this.Triggers().Inserting += e => e.Entity.Updated = DateTime.UtcNow;
			this.Triggers().Updating += e => e.Entity.Updated = DateTime.UtcNow;
		}
	}
}
