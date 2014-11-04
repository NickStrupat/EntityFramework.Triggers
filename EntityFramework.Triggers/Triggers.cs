using System;
using System.Data.Entity;
using System.Runtime.CompilerServices;

namespace EntityFramework.Triggers {
	public class Triggers<TTriggerable, TDbContext> : ITriggers<TDbContext>
		where TDbContext : DbContext
		where TTriggerable : class, ITriggerable {

		//internal static readonly ConditionalWeakTable<ITriggerable, ITriggers<TDbContext>> TriggersWeakRefs = new ConditionalWeakTable<ITriggerable, ITriggers<TDbContext>>(); 

		/// <summary>Contains the context and the instance of the changed entity</summary>
		public class Entry {
			internal Entry() {}
			public TTriggerable Entity { get; internal set; }
			public TDbContext Context { get; internal set; }
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

		void ITriggers<TDbContext>.OnBeforeInsert(ITriggerable triggerable, TDbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Inserting, new InsertingEntry { Entity = (TTriggerable) triggerable, Context = dbContext }); }
		void ITriggers<TDbContext>.OnBeforeUpdate(ITriggerable triggerable, TDbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Updating, new UpdatingEntry { Entity = (TTriggerable) triggerable, Context = dbContext }); }
		void ITriggers<TDbContext>.OnBeforeDelete(ITriggerable triggerable, TDbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Deleting, new DeletingEntry { Entity = (TTriggerable) triggerable, Context = dbContext }); }
		void ITriggers<TDbContext>.OnInsertFailed(ITriggerable triggerable, TDbContext dbContext, Exception exception) { RaiseDbEntityEntriesChangeEvent(InsertFailed, new FailedEntry { Entity = (TTriggerable) triggerable, Context = dbContext, Exception = exception }); }
		void ITriggers<TDbContext>.OnUpdateFailed(ITriggerable triggerable, TDbContext dbContext, Exception exception) { RaiseDbEntityEntriesChangeEvent(UpdateFailed, new FailedEntry { Entity = (TTriggerable) triggerable, Context = dbContext, Exception = exception }); }
		void ITriggers<TDbContext>.OnDeleteFailed(ITriggerable triggerable, TDbContext dbContext, Exception exception) { RaiseDbEntityEntriesChangeEvent(DeleteFailed, new FailedEntry { Entity = (TTriggerable) triggerable, Context = dbContext, Exception = exception }); }
		void ITriggers<TDbContext>.OnAfterInsert(ITriggerable triggerable, TDbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Inserted, new Entry { Entity = (TTriggerable) triggerable, Context = dbContext }); }
		void ITriggers<TDbContext>.OnAfterUpdate(ITriggerable triggerable, TDbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Updated, new Entry { Entity = (TTriggerable) triggerable, Context = dbContext }); }
		void ITriggers<TDbContext>.OnAfterDelete(ITriggerable triggerable, TDbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Deleted, new Entry { Entity = (TTriggerable) triggerable, Context = dbContext }); }
	}
}