using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFramework.Triggers {
	/// <summary>
	/// A <see cref="System.Data.Entity.DbContext"/> class with <see cref="Triggers{TTriggerable}"/> support
	/// </summary>
	public abstract class DbContextWithTriggers : DbContext {
		public override Int32 SaveChanges() {
			return this.SaveChangesWithTriggers();
		}
#if !NET40
		public override Task<Int32> SaveChangesAsync(CancellationToken cancellationToken) {
			return this.SaveChangesWithTriggersAsync(cancellationToken);
		}
#endif

        //
        // Summary:
        //     Constructs a new context instance using conventions to create the name of the
        //     database to which a connection will be made. The by-convention name is the full
        //     name (namespace + class name) of the derived context class. See the class remarks
        //     for how this is used to create a connection.
        protected DbContextWithTriggers() : base() { }
        //
        // Summary:
        //     Constructs a new context instance using conventions to create the name of the
        //     database to which a connection will be made, and initializes it from the given
        //     model. The by-convention name is the full name (namespace + class name) of the
        //     derived context class. See the class remarks for how this is used to create a
        //     connection.
        //
        // Parameters:
        //   model:
        //     The model that will back this context.
        protected DbContextWithTriggers(DbCompiledModel model) : base(model) { }
        //
        // Summary:
        //     Constructs a new context instance using the given string as the name or connection
        //     string for the database to which a connection will be made. See the class remarks
        //     for how this is used to create a connection.
        //
        // Parameters:
        //   nameOrConnectionString:
        //     Either the database name or a connection string.
        public DbContextWithTriggers(String nameOrConnectionString) : base(nameOrConnectionString) { }
        //
        // Summary:
        //     Constructs a new context instance using the existing connection to connect to
        //     a database. The connection will not be disposed when the context is disposed
        //     if contextOwnsConnection is false.
        //
        // Parameters:
        //   existingConnection:
        //     An existing connection to use for the new context.
        //
        //   contextOwnsConnection:
        //     If set to true the connection is disposed when the context is disposed, otherwise
        //     the caller must dispose the connection.
        public DbContextWithTriggers(DbConnection existingConnection, Boolean contextOwnsConnection) : base(existingConnection, contextOwnsConnection) { }
        //
        // Summary:
        //     Constructs a new context instance around an existing ObjectContext.
        //
        // Parameters:
        //   objectContext:
        //     An existing ObjectContext to wrap with the new context.
        //
        //   dbContextOwnsObjectContext:
        //     If set to true the ObjectContext is disposed when the DbContext is disposed,
        //     otherwise the caller must dispose the connection.
        public DbContextWithTriggers(ObjectContext objectContext, bool dbContextOwnsObjectContext) : base(objectContext, dbContextOwnsObjectContext) { }
        //
        // Summary:
        //     Constructs a new context instance using the given string as the name or connection
        //     string for the database to which a connection will be made, and initializes it
        //     from the given model. See the class remarks for how this is used to create a
        //     connection.
        //
        // Parameters:
        //   nameOrConnectionString:
        //     Either the database name or a connection string.
        //
        //   model:
        //     The model that will back this context.
        public DbContextWithTriggers(String nameOrConnectionString, DbCompiledModel model) : base(nameOrConnectionString, model) { }
        //
        // Summary:
        //     Constructs a new context instance using the existing connection to connect to
        //     a database, and initializes it from the given model. The connection will not
        //     be disposed when the context is disposed if contextOwnsConnection is false.
        //
        // Parameters:
        //   existingConnection:
        //     An existing connection to use for the new context.
        //
        //   model:
        //     The model that will back this context.
        //
        //   contextOwnsConnection:
        //     If set to true the connection is disposed when the context is disposed, otherwise
        //     the caller must dispose the connection.
        public DbContextWithTriggers(DbConnection existingConnection, DbCompiledModel model, Boolean contextOwnsConnection) : base(existingConnection, model, contextOwnsConnection) { }
    }
}
