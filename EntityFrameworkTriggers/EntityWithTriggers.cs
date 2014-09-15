using System;
using System.Data.Entity.Infrastructure;

namespace EntityFrameworkTriggers {
    /// <summary>Base class for entities which need events to fire before and after being added to, modified in, or removed from the store</summary>
    /// <typeparam name="TEntity">Derived entity class (see: CRTP)</typeparam>
    /// <typeparam name="TContext">Derived context class (see: CRTP)</typeparam>
    public abstract class EntityWithTriggers<TEntity, TContext> : IEntityWithTriggers<TContext>
		where TEntity : EntityWithTriggers<TEntity, TContext>
		where TContext : DbContextWithTriggers<TContext> {
        /// <summary>Contains the context and the instance of the changed entity</summary>
        public class Entry {
            /// <summary></summary>
			public TContext Context { get; internal set; }
            /// <summary></summary>
			public TEntity Entity { get; internal set; }
	        internal Entry() {}
        }

		public class FailedEntry : Entry {
			public Exception Exception { get; internal set; }
		}

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

        private void RaiseDbEntityEntriesChangeEvent(Action<Entry> eventHandler, TContext context) {
            if (eventHandler != null)
                eventHandler(new Entry { Context = context, Entity = (TEntity) this});
        }

		private void RaiseDbEntityEntriesFailedEvent(Action<FailedEntry> eventHandler, TContext dbContext, Exception exception) {
			if (eventHandler != null)
				eventHandler(new FailedEntry { Context = dbContext, Entity = (TEntity)this, Exception = exception});
		}

        void ITriggers<TContext>.OnBeforeInsert(TContext context) { RaiseDbEntityEntriesChangeEvent(Inserting, context); }
        void ITriggers<TContext>.OnBeforeUpdate(TContext context) { RaiseDbEntityEntriesChangeEvent(Updating, context); }
		void ITriggers<TContext>.OnBeforeDelete(TContext context) { RaiseDbEntityEntriesChangeEvent(Deleting, context); }
		void ITriggers<TContext>.OnInsertFailed(TContext dbContext, Exception exception) { RaiseDbEntityEntriesFailedEvent(InsertFailed, dbContext, exception); }
		void ITriggers<TContext>.OnUpdateFailed(TContext dbContext, Exception exception) { RaiseDbEntityEntriesFailedEvent(UpdateFailed, dbContext, exception); }
		void ITriggers<TContext>.OnDeleteFailed(TContext dbContext, Exception exception) { RaiseDbEntityEntriesFailedEvent(DeleteFailed, dbContext, exception); }
        void ITriggers<TContext>.OnAfterInsert(TContext context) { RaiseDbEntityEntriesChangeEvent(Inserted, context); }
        void ITriggers<TContext>.OnAfterUpdate(TContext context) { RaiseDbEntityEntriesChangeEvent(Updated, context); }
        void ITriggers<TContext>.OnAfterDelete(TContext context) { RaiseDbEntityEntriesChangeEvent(Deleted, context); }
    }
}
