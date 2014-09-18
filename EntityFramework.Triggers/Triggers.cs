using System;
using System.Data.Entity;

namespace EntityFramework.Triggers {
	public abstract class Triggers<TTriggerable, TDbContext> : ITriggers<TDbContext>
		where TDbContext : DbContext
		where TTriggerable : class, new() {
		/// <summary>Contains the context and the instance of the changed entity</summary>
		public class Entry {
			/// <summary></summary>
			public TDbContext Context { get; internal set; }
			public TDbContextDerived GetContext<TDbContextDerived>() where TDbContextDerived : TDbContext {
				return (TDbContextDerived) Context;
			}
			/// <summary></summary>
			public TTriggerable Entity { get; internal set; }
			internal Entry() {}
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

		private void RaiseDbEntityEntriesChangeEvent(Action<Entry> eventHandler, TDbContext dbContext) {
			if (eventHandler != null)
				eventHandler(new Entry { Context = dbContext, Entity = Triggerable });
		}

		private void RaiseDbEntityEntriesFailedEvent(Action<FailedEntry> eventHandler, TDbContext dbContext, Exception exception) {
			if (eventHandler != null)
				eventHandler(new FailedEntry { Context = dbContext, Entity = Triggerable, Exception = exception });
		}

		void ITriggers<TDbContext>.OnBeforeInsert(TDbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Inserting, dbContext); }
		void ITriggers<TDbContext>.OnBeforeUpdate(TDbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Updating, dbContext); }
		void ITriggers<TDbContext>.OnBeforeDelete(TDbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Deleting, dbContext); }
		void ITriggers<TDbContext>.OnInsertFailed(TDbContext dbContext, Exception exception) { RaiseDbEntityEntriesFailedEvent(InsertFailed, dbContext, exception); }
		void ITriggers<TDbContext>.OnUpdateFailed(TDbContext dbContext, Exception exception) { RaiseDbEntityEntriesFailedEvent(UpdateFailed, dbContext, exception); }
		void ITriggers<TDbContext>.OnDeleteFailed(TDbContext dbContext, Exception exception) { RaiseDbEntityEntriesFailedEvent(DeleteFailed, dbContext, exception); }
		void ITriggers<TDbContext>.OnAfterInsert(TDbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Inserted, dbContext); }
		void ITriggers<TDbContext>.OnAfterUpdate(TDbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Updated, dbContext); }
		void ITriggers<TDbContext>.OnAfterDelete(TDbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Deleted, dbContext); }
	}

	public class Triggers<TTriggerable> : Triggers<TTriggerable, DbContext> where TTriggerable : class, ITriggerable<TTriggerable>, new() {
		
	}
}