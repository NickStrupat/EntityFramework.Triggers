using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;

namespace EntityFramework.Triggers {
	/// <summary>
	/// A <see cref="System.Data.Entity.DbContext"/> class with <see cref="Triggers{TTriggerable}"/> support
	/// </summary>
	public abstract class DbContextWithTriggers : DbContext {
		public override Int32 SaveChanges() {
			return this.SaveChangesWithTriggers(base.SaveChanges);
		}
#if !NET40
		public override Task<Int32> SaveChangesAsync(CancellationToken cancellationToken) {
			return this.SaveChangesWithTriggersAsync(base.SaveChangesAsync, cancellationToken);
		}
#endif

		/// <summary>
		///     Constructs a new context instance using conventions to create the name of the
		///     database to which a connection will be made. The by-convention name is the full
		///     name (namespace + class name) of the derived context class. See the class remarks
		///     for how this is used to create a connection.
		/// </summary>
		protected DbContextWithTriggers() : base() { }

		/// <summary>
		///     Constructs a new context instance using conventions to create the name of the
		///     database to which a connection will be made, and initializes it from the given
		///     model. The by-convention name is the full name (namespace + class name) of the
		///     derived context class. See the class remarks for how this is used to create a
		///     connection.
		/// </summary>
		/// <param name="model">The model that will back this context.</param>
		protected DbContextWithTriggers(DbCompiledModel model) : base(model) { }

		/// <summary>
		///     Constructs a new context instance using the given string as the name or connection
		///     string for the database to which a connection will be made. See the class remarks
		///     for how this is used to create a connection.
		/// </summary>
		/// <param name="nameOrConnectionString">Either the database name or a connection string.</param>
		protected DbContextWithTriggers(String nameOrConnectionString) : base(nameOrConnectionString) { }

		/// <summary>
		///     Constructs a new context instance using the existing connection to connect to
		///     a database. The connection will not be disposed when the context is disposed
		///     if contextOwnsConnection is false.
		/// </summary>
		/// <param name="existingConnection">An existing connection to use for the new context.</param>
		/// <param name="contextOwnsConnection">If set to true the connection is disposed when the context is disposed, otherwise the caller must dispose the connection.</param>
		protected DbContextWithTriggers(DbConnection existingConnection, Boolean contextOwnsConnection) : base(existingConnection, contextOwnsConnection) { }

		/// <summary>
		///     Constructs a new context instance around an existing ObjectContext.
		/// </summary>
		/// <param name="objectContext">An existing ObjectContext to wrap with the new context.</param>
		/// <param name="dbContextOwnsObjectContext">If set to true the ObjectContext is disposed when the DbContext is disposed, otherwise the caller must dispose the connection.</param>
		protected DbContextWithTriggers(ObjectContext objectContext, Boolean dbContextOwnsObjectContext) : base(objectContext, dbContextOwnsObjectContext) { }

		/// <summary>
		///     Constructs a new context instance using the given string as the name or connection
		///     string for the database to which a connection will be made, and initializes it
		///     from the given model. See the class remarks for how this is used to create a
		///     connection.
		/// </summary>
		/// <param name="nameOrConnectionString">Either the database name or a connection string.</param>
		/// <param name="model">The model that will back this context.</param>
		protected DbContextWithTriggers(String nameOrConnectionString, DbCompiledModel model) : base(nameOrConnectionString, model) { }

		/// <summary>
		///     Constructs a new context instance using the existing connection to connect to
		///     a database, and initializes it from the given model. The connection will not
		///     be disposed when the context is disposed if contextOwnsConnection is false.
		/// </summary>
		/// <param name="existingConnection">An existing connection to use for the new context.</param>
		/// <param name="model">The model that will back this context.</param>
		/// <param name="contextOwnsConnection">If set to true the connection is disposed when the context is disposed, otherwise the caller must dispose the connection.</param>
		protected DbContextWithTriggers(DbConnection existingConnection, DbCompiledModel model, Boolean contextOwnsConnection) : base(existingConnection, model, contextOwnsConnection) { }
	}
}
