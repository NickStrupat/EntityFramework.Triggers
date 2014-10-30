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
			internal FailedEntry() { }
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

		internal TTriggerable Triggerable;
		
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

		private void RaiseDbEntityEntriesChangeEvent<TEntry>(Action<TEntry> eventHandler, TEntry entry) where TEntry : Entry {
			entry.Entity = Triggerable;
			if (eventHandler != null)
				eventHandler(entry);
		}

		void ITriggers<TDbContext>.OnBeforeInsert(TDbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Inserting, new InsertingEntry { Context = dbContext }); }
		void ITriggers<TDbContext>.OnBeforeUpdate(TDbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Updating, new UpdatingEntry { Context = dbContext }); }
		void ITriggers<TDbContext>.OnBeforeDelete(TDbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Deleting, new DeletingEntry { Context = dbContext }); }
		void ITriggers<TDbContext>.OnInsertFailed(TDbContext dbContext, Exception exception) { RaiseDbEntityEntriesChangeEvent(InsertFailed, new FailedEntry { Context = dbContext, Exception = exception }); }
		void ITriggers<TDbContext>.OnUpdateFailed(TDbContext dbContext, Exception exception) { RaiseDbEntityEntriesChangeEvent(UpdateFailed, new FailedEntry { Context = dbContext, Exception = exception }); }
		void ITriggers<TDbContext>.OnDeleteFailed(TDbContext dbContext, Exception exception) { RaiseDbEntityEntriesChangeEvent(DeleteFailed, new FailedEntry { Context = dbContext, Exception = exception }); }
		void ITriggers<TDbContext>.OnAfterInsert(TDbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Inserted, new Entry { Context = dbContext }); }
		void ITriggers<TDbContext>.OnAfterUpdate(TDbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Updated, new Entry { Context = dbContext }); }
		void ITriggers<TDbContext>.OnAfterDelete(TDbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Deleted, new Entry { Context = dbContext }); }
	}

	public class Triggers<TTriggerable> : Triggers<TTriggerable, DbContext> where TTriggerable : class, ITriggerable<TTriggerable>, new() {
		
	}
}