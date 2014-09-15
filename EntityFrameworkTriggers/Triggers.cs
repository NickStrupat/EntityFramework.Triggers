using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace EntityFrameworkTriggers {
	public class Triggers<TTriggerable> : ITriggers where TTriggerable : class, ITriggerable<TTriggerable>, new() {
		/// <summary>Contains the context and the instance of the changed entity</summary>
		public class Entry {
			/// <summary></summary>
			public DbContext Context { get; internal set; }
			public TDbContext GetContext<TDbContext>() where TDbContext : DbContext {
				return (TDbContext) Context;
			}
			/// <summary></summary>
			public TTriggerable Entity { get; internal set; }
		}

		public class FailedEntry : Entry {
			public Exception Exception { get; internal set; }
		}

		internal TTriggerable Triggerable;
		
		/// <summary>Raised just before this entity is added to the store</summary>
		public event Action<Entry> Inserting;

		/// <summary>Raised just before this entity is updated in the store</summary>
		public event Action<Entry> Updating;

		/// <summary>Raised just before this entity is deleted from the store</summary>
		public event Action<Entry> Deleting;

		/// <summary>Raised after Inserting event, but before Inserted event when an exception has occured while saving the changes to the store</summary>
		public event Action<FailedEntry> InsertFailed;

		/// <summary>Raised after Updating event, but before Updated event when an exception has occured while saving the changes to the store</summary>
		public event Action<FailedEntry> UpdateFailed;

		/// <summary>Raised after Deleting event, but before Deleted event when an exception has occured while saving the changes to the store</summary>
		public event Action<FailedEntry> DeleteFailed;

		/// <summary>Raised just after this entity is added to the store</summary>
		public event Action<Entry> Inserted;

		/// <summary>Raised just after this entity is updated in the store</summary>
		public event Action<Entry> Updated;

		/// <summary>Raised just after this entity is deleted from the store</summary>
		public event Action<Entry> Deleted;

		private void RaiseDbEntityEntriesChangeEvent(Action<Entry> eventHandler, DbContext dbcontext) {
			if (eventHandler != null)
				eventHandler(new Entry { Context = dbcontext, Entity = Triggerable });
		}

		private void RaiseDbEntityEntriesFailedEvent(Action<FailedEntry> eventHandler, DbContext dbcontext, Exception exception) {
			if (eventHandler != null)
				eventHandler(new FailedEntry { Context = dbcontext, Entity = Triggerable, Exception = exception });
		}

		void ITriggers<DbContext>.OnBeforeInsert(DbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Inserting, dbContext); }
		void ITriggers<DbContext>.OnBeforeUpdate(DbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Updating, dbContext); }
		void ITriggers<DbContext>.OnBeforeDelete(DbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Deleting, dbContext); }
		void ITriggers<DbContext>.OnInsertFailed(DbContext dbContext, Exception exception) { RaiseDbEntityEntriesFailedEvent(InsertFailed, dbContext, exception); }
		void ITriggers<DbContext>.OnUpdateFailed(DbContext dbContext, Exception exception) { RaiseDbEntityEntriesFailedEvent(UpdateFailed, dbContext, exception); }
		void ITriggers<DbContext>.OnDeleteFailed(DbContext dbContext, Exception exception) { RaiseDbEntityEntriesFailedEvent(DeleteFailed, dbContext, exception); }
		void ITriggers<DbContext>.OnAfterInsert(DbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Inserted, dbContext); }
		void ITriggers<DbContext>.OnAfterUpdate(DbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Updated, dbContext); }
		void ITriggers<DbContext>.OnAfterDelete(DbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Deleted, dbContext); }
	}
}