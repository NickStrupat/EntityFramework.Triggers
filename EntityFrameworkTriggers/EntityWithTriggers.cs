namespace EntityFrameworkTriggers {
    public abstract class EntityWithTriggers<T> : IEntityWithTriggers where T : EntityWithTriggers<T> {
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
