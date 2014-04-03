using System.Data.Entity;

namespace EntityFrameworkTriggers {
    /// <summary>Base class for entities which need events to fire before and after being added to, modified in, or removed from the store</summary>
    /// <typeparam name="TEntity">Derived entity class (see: CRTP)</typeparam>
    /// <typeparam name="TContext">Derived context class (see: CRTP)</typeparam>
    public abstract class EntityWithTriggers<TEntity, TContext> : IEntityWithTriggers<TContext> where TEntity : EntityWithTriggers<TEntity, TContext> where TContext : DbContextWithTriggers<TContext> {
        /// <param name="context">The context</param>
        /// <param name="entity">The instance of the changed entity</param>
        public delegate void DbEntityEntriesChangeEventHandler(TContext context, TEntity entity);

        /// <summary>Fired just before this entity is added to the store</summary>
        public event DbEntityEntriesChangeEventHandler Inserting;

        /// <summary>Fired just before this entity is updated in the store</summary>
        public event DbEntityEntriesChangeEventHandler Updating;

        /// <summary>Fired just before this entity is deleted from the store</summary>
        public event DbEntityEntriesChangeEventHandler Deleting;

        /// <summary>Fired just after this entity is added to the store</summary>
        public event DbEntityEntriesChangeEventHandler Inserted;

        /// <summary>Fired just after this entity is updated in the store</summary>
        public event DbEntityEntriesChangeEventHandler Updated;

        /// <summary>Fired just after this entity is deleted from the store</summary>
        public event DbEntityEntriesChangeEventHandler Deleted;

        private void RaiseDbEntityEntriesChangeEvent(DbEntityEntriesChangeEventHandler eventHandler, TContext context) {
            if (eventHandler != null)
                eventHandler(context, (TEntity) this);
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
