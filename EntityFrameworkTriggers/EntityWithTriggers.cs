namespace EntityFrameworkTriggers {
    /// <summary>Base class for entities which need events to fire before and after being added to, modified in, or removed from the store</summary>
    /// <typeparam name="T">Derived class (see: CRTP)</typeparam>
    public abstract class EntityWithTriggers<T> : IEntityWithTriggers where T : EntityWithTriggers<T> {
        /// <param name="entity">The instance of the changed entity</param>
        public delegate void DbEntityEntriesChangeEventHandler(T entity);

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

        private void RaiseDbEntityEntriesChangeEvent(DbEntityEntriesChangeEventHandler eventHandler) {
            if (eventHandler != null)
                eventHandler((T) this);
        }
        void IEntityWithTriggers.OnBeforeInsert() { RaiseDbEntityEntriesChangeEvent(Inserting); }
        void IEntityWithTriggers.OnBeforeUpdate() { RaiseDbEntityEntriesChangeEvent(Updating); }
        void IEntityWithTriggers.OnBeforeDelete() { RaiseDbEntityEntriesChangeEvent(Deleting); }
        void IEntityWithTriggers.OnAfterInsert() { RaiseDbEntityEntriesChangeEvent(Inserted); }
        void IEntityWithTriggers.OnAfterUpdate() { RaiseDbEntityEntriesChangeEvent(Updated); }
        void IEntityWithTriggers.OnAfterDelete() { RaiseDbEntityEntriesChangeEvent(Deleted); }
    }

    internal interface IEntityWithTriggers {
        void OnBeforeInsert();
        void OnBeforeUpdate();
        void OnBeforeDelete();
        void OnAfterInsert();
        void OnAfterUpdate();
        void OnAfterDelete();
    }
}
