using System;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFramework.Triggers {
    /// <summary>Base class to enable 'EntityWithTriggers' events. Derive your context class from this class to have those events raised accordingly when 'SaveChanges' and 'SaveChangesAsync' are invoked</summary>
	public abstract class DbContextWithTriggers<TDbContext> : DbContext where TDbContext : DbContextWithTriggers<TDbContext> {
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
        protected DbContextWithTriggers(String nameOrConnectionString) : base(nameOrConnectionString) {}
        /// <summary>
        /// Constructs a new context instance using the given string as the name or connection string for the
        ///             database to which a connection will be made, and initializes it from the given model.
        ///             See the class remarks for how this is used to create a connection.
        /// 
        /// </summary>
        /// <param name="nameOrConnectionString">Either the database name or a connection string. </param><param name="model">The model that will back this context. </param>
        protected DbContextWithTriggers(String nameOrConnectionString, DbCompiledModel model) : base(nameOrConnectionString, model) {}
        /// <summary>
        /// Constructs a new context instance using the existing connection to connect to a database.
        ///             The connection will not be disposed when the context is disposed if <paramref name="contextOwnsConnection"/>
        ///             is <c>false</c>.
        /// 
        /// </summary>
        /// <param name="existingConnection">An existing connection to use for the new context. </param><param name="contextOwnsConnection">If set to <c>true</c> the connection is disposed when the context is disposed, otherwise the caller must dispose the connection.
        ///             </param>
        protected DbContextWithTriggers(System.Data.Common.DbConnection existingConnection, Boolean contextOwnsConnection) : base(existingConnection, contextOwnsConnection) {}
        /// <summary>
        /// Constructs a new context instance using the existing connection to connect to a database,
        ///             and initializes it from the given model.
        ///             The connection will not be disposed when the context is disposed if <paramref name="contextOwnsConnection"/>
        ///             is <c>false</c>.
        /// 
        /// </summary>
        /// <param name="existingConnection">An existing connection to use for the new context. </param><param name="model">The model that will back this context. </param><param name="contextOwnsConnection">If set to <c>true</c> the connection is disposed when the context is disposed, otherwise the caller must dispose the connection.
        ///             </param>
        protected DbContextWithTriggers(System.Data.Common.DbConnection existingConnection, DbCompiledModel model, Boolean contextOwnsConnection) : base(existingConnection, model, contextOwnsConnection) {}
        /// <summary>
        /// Constructs a new context instance around an existing ObjectContext.
        /// 
        /// </summary>
        /// <param name="objectContext">An existing ObjectContext to wrap with the new context. </param><param name="dbContextOwnsObjectContext">If set to <c>true</c> the ObjectContext is disposed when the DbContext is disposed, otherwise the caller must dispose the connection.
        ///             </param>
        protected DbContextWithTriggers(ObjectContext objectContext, Boolean dbContextOwnsObjectContext) : base(objectContext, dbContextOwnsObjectContext) {}

        /// <summary>
        /// Saves all changes made in this context to the underlying database, while raising the events of all tracked entities which inherit from ITriggerable.
        /// </summary>
        /// <returns>
        /// The number of objects written to the underlying database. 
        /// </returns>
        public override Int32 SaveChanges() {
	        return ((TDbContext)this).SaveChangesWithTriggers(base.SaveChanges);
        }
        /// <summary>
		/// Asynchronously saves all changes made in this context to the underlying database, while raising the events of all tracked entities which inherit from ITriggerable.
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
        public override Task<Int32> SaveChangesAsync(CancellationToken cancellationToken) {
			return ((TDbContext)this).SaveChangesWithTriggersAsync(base.SaveChangesAsync, cancellationToken);
        }
    }
}