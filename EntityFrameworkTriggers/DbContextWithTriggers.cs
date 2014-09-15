using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkTriggers {
    /// <summary>Base class to enable 'EntityWithTriggers' events. Derive your context class from this class to have those events raised accordingly when 'SaveChanges' and 'SaveChangesAsync' are invoked</summary>
    public abstract class DbContextWithTriggers<TContext> : DbContext where TContext : DbContextWithTriggers<TContext> {
        /// <summary>
        /// Constructs a new context instance using conventions to create the name of the database to
        ///             which a connection will be made.  The by-convention name is the full name (namespace + class name)
        ///             of the derived context class.
        ///             See the class remarks for how this is used to create a connection.
        /// 
        /// </summary>
        protected DbContextWithTriggers() : base() {}
        /// <summary>
        /// Constructs a new context instance using conventions to create the name of the database to
        ///             which a connection will be made, and initializes it from the given model.
        ///             The by-convention name is the full name (namespace + class name) of the derived context class.
        ///             See the class remarks for how this is used to create a connection.
        /// 
        /// </summary>
        /// <param name="model">The model that will back this context. </param>
        protected DbContextWithTriggers(DbCompiledModel model) : base(model) {}
        /// <summary>
        /// Constructs a new context instance using the given string as the name or connection string for the
        ///             database to which a connection will be made.
        ///             See the class remarks for how this is used to create a connection.
        /// 
        /// </summary>
        /// <param name="nameOrConnectionString">Either the database name or a connection string. </param>
        public DbContextWithTriggers(String nameOrConnectionString) : base(nameOrConnectionString) {}
        /// <summary>
        /// Constructs a new context instance using the given string as the name or connection string for the
        ///             database to which a connection will be made, and initializes it from the given model.
        ///             See the class remarks for how this is used to create a connection.
        /// 
        /// </summary>
        /// <param name="nameOrConnectionString">Either the database name or a connection string. </param><param name="model">The model that will back this context. </param>
        public DbContextWithTriggers(String nameOrConnectionString, DbCompiledModel model) : base(nameOrConnectionString, model) {}
        /// <summary>
        /// Constructs a new context instance using the existing connection to connect to a database.
        ///             The connection will not be disposed when the context is disposed if <paramref name="contextOwnsConnection"/>
        ///             is <c>false</c>.
        /// 
        /// </summary>
        /// <param name="existingConnection">An existing connection to use for the new context. </param><param name="contextOwnsConnection">If set to <c>true</c> the connection is disposed when the context is disposed, otherwise the caller must dispose the connection.
        ///             </param>
        public DbContextWithTriggers(System.Data.Common.DbConnection existingConnection, Boolean contextOwnsConnection) : base(existingConnection, contextOwnsConnection) {}
        /// <summary>
        /// Constructs a new context instance using the existing connection to connect to a database,
        ///             and initializes it from the given model.
        ///             The connection will not be disposed when the context is disposed if <paramref name="contextOwnsConnection"/>
        ///             is <c>false</c>.
        /// 
        /// </summary>
        /// <param name="existingConnection">An existing connection to use for the new context. </param><param name="model">The model that will back this context. </param><param name="contextOwnsConnection">If set to <c>true</c> the connection is disposed when the context is disposed, otherwise the caller must dispose the connection.
        ///             </param>
        public DbContextWithTriggers(System.Data.Common.DbConnection existingConnection, DbCompiledModel model, Boolean contextOwnsConnection) : base(existingConnection, model, contextOwnsConnection) {}
        /// <summary>
        /// Constructs a new context instance around an existing ObjectContext.
        /// 
        /// </summary>
        /// <param name="objectContext">An existing ObjectContext to wrap with the new context. </param><param name="dbContextOwnsObjectContext">If set to <c>true</c> the ObjectContext is disposed when the DbContext is disposed, otherwise the caller must dispose the connection.
        ///             </param>
        public DbContextWithTriggers(ObjectContext objectContext, Boolean dbContextOwnsObjectContext) : base(objectContext, dbContextOwnsObjectContext) {}

        private IEnumerable<Action<TContext>> RaiseTheBeforeEvents() {
            var afterActions = new List<Action<TContext>>();
            foreach (var entry in ChangeTracker.Entries<ITriggers<TContext>>()) {
                switch (entry.State) {
                    case EntityState.Added:
                        entry.Entity.OnBeforeInsert((TContext)this);
                        afterActions.Add(entry.Entity.OnAfterInsert);
                        break;
                    case EntityState.Deleted:
                        entry.Entity.OnBeforeDelete((TContext)this);
                        afterActions.Add(entry.Entity.OnAfterDelete);
                        break;
                    case EntityState.Modified:
                        entry.Entity.OnBeforeUpdate((TContext)this);
                        afterActions.Add(entry.Entity.OnAfterUpdate);
                        break;
                }
            }
            return afterActions;
        }

        private void RaiseTheAfterEvents(IEnumerable<Action<TContext>> afterActions) {
            foreach (var afterAction in afterActions)
                afterAction((TContext)this);
        }

		private void RaiseTheFailedEvents(Exception exception) {
			var dbUpdateException = exception as DbUpdateException;
			var dbEntityValidationException = exception as DbEntityValidationException;
			if (dbUpdateException != null) {
				foreach (var entry in dbUpdateException.Entries.Where(x => x.Entity is ITriggers<TContext>))
					RaiseTheFailedEvents(entry, dbUpdateException);
			}
			else if (dbEntityValidationException != null) {
				foreach (var dbEntityValidationResult in dbEntityValidationException.EntityValidationErrors.Where(x => x.Entry.Entity is ITriggers<TContext>))
					RaiseTheFailedEvents(dbEntityValidationResult.Entry, dbEntityValidationException);
			}
		}

	    private void RaiseTheFailedEvents(DbEntityEntry entry, Exception exception) {
			var entity = (ITriggers<TContext>)entry.Entity;
			switch (entry.State) {
				case EntityState.Added:
					entity.OnInsertFailed((TContext)this, exception);
					break;
				case EntityState.Modified:
					entity.OnUpdateFailed((TContext)this, exception);
					break;
				case EntityState.Deleted:
					entity.OnDeleteFailed((TContext)this, exception);
					break;
			}
		}

        /// <summary>
        /// Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>
        /// The number of objects written to the underlying database. 
        /// </returns>
        public override Int32 SaveChanges() {
            var afterActions = RaiseTheBeforeEvents();
			Int32 result;
			try {
				result = base.SaveChanges();
			}
			catch (Exception exception) {
				RaiseTheFailedEvents(exception);
				throw;
			}
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
        public override async Task<Int32> SaveChangesAsync(CancellationToken cancellationToken) {
            var afterActions = RaiseTheBeforeEvents();
			Int32 result;
			try {
				result = await base.SaveChangesAsync(cancellationToken);
			}
			catch (Exception exception) {
				RaiseTheFailedEvents(exception);
				throw;
			}
            RaiseTheAfterEvents(afterActions);
            return result;
        }
    }
}