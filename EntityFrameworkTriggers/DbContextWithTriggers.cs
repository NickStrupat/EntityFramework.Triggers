using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkTriggers {
    /// <summary>Base class to enable 'EntityWithTriggers' events. Derive your context class from this class to have those events raised accordingly when 'SaveChanges' and 'SaveChangesAsync' are invoked</summary>
    public abstract class DbContextWithTriggers : DbContext {
#pragma warning disable 1591
        protected DbContextWithTriggers() : base() {}
        protected DbContextWithTriggers(DbCompiledModel model) : base(model) {}
        public DbContextWithTriggers(String nameOrConnectionString) : base(nameOrConnectionString) {}
        public DbContextWithTriggers(ObjectContext objectContext, Boolean dbContextOwnsObjectContext) : base(objectContext, dbContextOwnsObjectContext) { }
        public DbContextWithTriggers(String nameOrConnectionString, DbCompiledModel model) : base(nameOrConnectionString, model) {}
        public DbContextWithTriggers(System.Data.Common.DbConnection existingConnection, Boolean contextOwnsConnection) : base(existingConnection, contextOwnsConnection) { }
        public DbContextWithTriggers(System.Data.Common.DbConnection existingConnection, DbCompiledModel model, Boolean contextOwnsConnection) : base(existingConnection, model, contextOwnsConnection) { }
#pragma warning restore 1591

        private IEnumerable<Action> RaiseTheBeforeEvents()
        {
            var afterActions = new List<Action>();
            foreach (var entry in ChangeTracker.Entries<IEntityWithTriggers>()) {
                switch (entry.State) {
                    case EntityState.Added:
                        entry.Entity.OnBeforeInsert();
                        afterActions.Add(entry.Entity.OnAfterInsert);
                        break;
                    case EntityState.Deleted:
                        entry.Entity.OnBeforeDelete();
                        afterActions.Add(entry.Entity.OnAfterDelete);
                        break;
                    case EntityState.Modified:
                        entry.Entity.OnBeforeUpdate();
                        afterActions.Add(entry.Entity.OnAfterUpdate);
                        break;
                }
            }
            return afterActions;
        }

        private void RaiseTheAfterEvents(IEnumerable<Action> afterActions)
        {
            foreach (var afterAction in afterActions)
                afterAction();
        }

        /// <summary>
        /// Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>
        /// The number of objects written to the underlying database. 
        /// </returns>
        /// <exception cref="T:System.Data.Entity.Infrastructure.DbUpdateException">An error occurred sending updates to the database.</exception><exception cref="T:System.Data.Entity.Infrastructure.DbUpdateConcurrencyException">A database command did not affect the expected number of rows. This usually indicates an optimistic 
        ///             concurrency violation; that is, a row has been changed in the database since it was queried.
        ///             </exception><exception cref="T:System.Data.Entity.Validation.DbEntityValidationException">The save was aborted because validation of entity property values failed.
        ///             </exception><exception cref="T:System.NotSupportedException">An attempt was made to use unsupported behavior such as executing multiple asynchronous commands concurrently
        ///             on the same context instance.</exception><exception cref="T:System.ObjectDisposedException">The context or connection have been disposed.</exception><exception cref="T:System.InvalidOperationException">Some error occurred attempting to process entities in the context either before or after sending commands
        ///             to the database.
        ///             </exception>
        public override Int32 SaveChanges() {
            var afterActions = RaiseTheBeforeEvents();
            var result = base.SaveChanges();
            RaiseTheAfterEvents(afterActions);
            return result;
        }

        /// <summary>
        /// Asynchronously saves all changes made in this context to the underlying database.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///             that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> to observe while waiting for the task to complete.
        ///             </param>
        /// <returns>
        /// A task that represents the asynchronous save operation.
        ///             The task result contains the number of objects written to the underlying database.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">Thrown if the context has been disposed.</exception>
        public override async Task<Int32> SaveChangesAsync(CancellationToken cancellationToken)
        {
            var afterActions = RaiseTheBeforeEvents();
            var result = await base.SaveChangesAsync(cancellationToken);
            RaiseTheAfterEvents(afterActions);
            return result;
        }
    }
}