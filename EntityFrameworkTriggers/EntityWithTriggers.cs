using System;

namespace EntityFrameworkTriggers {
    /// <summary>Base class for entities which need events to fire before and after being added to, modified in, or removed from the store</summary>
    /// <typeparam name="TEntity">Derived entity class (see: CRTP)</typeparam>
    /// <typeparam name="TContext">Derived context class (see: CRTP)</typeparam>
    public abstract class EntityWithTriggers<TEntity, TContext> : IEntityWithTriggers<TContext> where TEntity : EntityWithTriggers<TEntity, TContext> where TContext : DbContextWithTriggers<TContext> {
        /// <summary>Contains the context and the instance of the changed entity</summary>
        public struct Entry {
            /// <summary></summary>
            public TContext Context { get; private set; }
            /// <summary></summary>
            public TEntity Entity { get; private set; }
            internal Entry(TContext context, TEntity entity) : this() {
                Context = context;
                Entity = entity;
            }
        }

        /// <summary>Raised just before this entity is added to the store</summary>
        public event Action<Entry> Inserting;

        /// <summary>Raised just before this entity is updated in the store</summary>
        public event Action<Entry> Updating;

        /// <summary>Raised just before this entity is deleted from the store</summary>
        public event Action<Entry> Deleting;

        /// <summary>Raised just after this entity is added to the store</summary>
        public event Action<Entry> Inserted;

        /// <summary>Raised just after this entity is updated in the store</summary>
        public event Action<Entry> Updated;

        /// <summary>Raised just after this entity is deleted from the store</summary>
        public event Action<Entry> Deleted;

        private void RaiseDbEntityEntriesChangeEvent(Action<Entry> eventHandler, TContext context) {
            if (eventHandler != null)
                eventHandler(new Entry(context, (TEntity) this));
        }
        void IEntityWithTriggers<TContext>.OnBeforeInsert(TContext context) { RaiseDbEntityEntriesChangeEvent(Inserting, context); }
        void IEntityWithTriggers<TContext>.OnBeforeUpdate(TContext context) { RaiseDbEntityEntriesChangeEvent(Updating, context); }
        void IEntityWithTriggers<TContext>.OnBeforeDelete(TContext context) { RaiseDbEntityEntriesChangeEvent(Deleting, context); }
        void IEntityWithTriggers<TContext>.OnAfterInsert(TContext context) { RaiseDbEntityEntriesChangeEvent(Inserted, context); }
        void IEntityWithTriggers<TContext>.OnAfterUpdate(TContext context) { RaiseDbEntityEntriesChangeEvent(Updated, context); }
        void IEntityWithTriggers<TContext>.OnAfterDelete(TContext context) { RaiseDbEntityEntriesChangeEvent(Deleted, context); }
    }

    internal interface IEntityWithTriggers<in TContext> where TContext : DbContextWithTriggers<TContext> {
        void OnBeforeInsert(TContext context);
        void OnBeforeUpdate(TContext context);
        void OnBeforeDelete(TContext context);
        void OnAfterInsert(TContext context);
        void OnAfterUpdate(TContext context);
        void OnAfterDelete(TContext context);
    }
}
