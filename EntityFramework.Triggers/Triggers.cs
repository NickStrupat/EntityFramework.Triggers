using System;
using System.Data.Entity;

namespace EntityFramework.Triggers {
	public class Triggers<TTriggerable> : ITriggers where TTriggerable : class, ITriggerable {
		/// <summary>Contains the context and the instance of the changed entity</summary>
		public class Entry {
			internal Entry() {}
			public TTriggerable Entity { get; internal set; }
			public DbContext Context { get; internal set; }
		}
		public class FailedEntry : Entry {
			internal FailedEntry() {}
			public Exception Exception { get; internal set; }
		}
		public abstract class BeforeEntry : Entry {
			public virtual void Cancel() {
				Context.Entry(Entity).State = EntityState.Unchanged;
			}
		}
		public class InsertingEntry : BeforeEntry {
			internal InsertingEntry() { }
		}
		public class UpdatingEntry : BeforeEntry {
			internal UpdatingEntry() { }
		}
		public class DeletingEntry : BeforeEntry {
			internal DeletingEntry() { }
			public override void Cancel() {
				Context.Entry(Entity).State = EntityState.Modified;
			}
		}
		
		/// <summary>Raised just before this entity is added to the store</summary>
		public event Action<BeforeEntry> Inserting;

		/// <summary>Raised just before this entity is updated in the store</summary>
		public event Action<BeforeEntry> Updating;

		/// <summary>Raised just before this entity is deleted from the store</summary>
		public event Action<BeforeEntry> Deleting;

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

		private static void RaiseDbEntityEntriesChangeEvent<TEntry>(Action<TEntry> eventHandler, TEntry entry) where TEntry : Entry {
			if (eventHandler != null)
				eventHandler(entry);
		}

		void ITriggers.OnBeforeInsert(ITriggerable triggerable, DbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Inserting, new InsertingEntry { Entity = (TTriggerable) triggerable, Context = dbContext }); }
		void ITriggers.OnBeforeUpdate(ITriggerable triggerable, DbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Updating, new UpdatingEntry { Entity = (TTriggerable) triggerable, Context = dbContext }); }
		void ITriggers.OnBeforeDelete(ITriggerable triggerable, DbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Deleting, new DeletingEntry { Entity = (TTriggerable) triggerable, Context = dbContext }); }
		void ITriggers.OnInsertFailed(ITriggerable triggerable, DbContext dbContext, Exception exception) { RaiseDbEntityEntriesChangeEvent(InsertFailed, new FailedEntry { Entity = (TTriggerable) triggerable, Context = dbContext, Exception = exception }); }
		void ITriggers.OnUpdateFailed(ITriggerable triggerable, DbContext dbContext, Exception exception) { RaiseDbEntityEntriesChangeEvent(UpdateFailed, new FailedEntry { Entity = (TTriggerable) triggerable, Context = dbContext, Exception = exception }); }
		void ITriggers.OnDeleteFailed(ITriggerable triggerable, DbContext dbContext, Exception exception) { RaiseDbEntityEntriesChangeEvent(DeleteFailed, new FailedEntry { Entity = (TTriggerable) triggerable, Context = dbContext, Exception = exception }); }
		void ITriggers.OnAfterInsert(ITriggerable triggerable, DbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Inserted, new Entry { Entity = (TTriggerable) triggerable, Context = dbContext }); }
		void ITriggers.OnAfterUpdate(ITriggerable triggerable, DbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Updated, new Entry { Entity = (TTriggerable) triggerable, Context = dbContext }); }
		void ITriggers.OnAfterDelete(ITriggerable triggerable, DbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Deleted, new Entry { Entity = (TTriggerable) triggerable, Context = dbContext }); }
	}
}