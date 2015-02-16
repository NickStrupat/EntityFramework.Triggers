using System;
using EntityFramework.Triggers;

namespace Tests {
	public abstract class EntityWithInsertTracking : ITriggerable {
		public DateTime Inserted { get; protected set; }
		public Int32 Number { get; protected set; }
		protected EntityWithInsertTracking() {
			this.Triggers().Inserting += e => e.Entity.Inserted = DateTime.UtcNow;
			this.Triggers().Inserting += e => e.Entity.Number = 42;
		}
	}
	public abstract class EntityWithTracking : EntityWithInsertTracking {
		public DateTime Updated { get; protected set; }
		protected EntityWithTracking() {
			this.Triggers().Inserting += e => e.Entity.Updated = DateTime.UtcNow;
			this.Triggers().Updating += e => e.Entity.Updated = DateTime.UtcNow;
		}
	}
}
