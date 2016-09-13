using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers {

	/// <summary>
	/// A <see cref="DbContext"/>-derived class with trigger functionality called automatically
	/// </summary>
	public abstract class DbContextWithTriggers : DbContext {
		/// <summary>
		///     Saves all changes made in this context to the database.
		/// </summary>
		/// <remarks>
		///     This method will automatically call <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges" /> to discover any
		///     changes to entity instances before saving to the underlying database. This can be disabled via
		///     <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled" />.
		/// </remarks>
		/// <returns>
		///     The number of state entries written to the database.
		/// </returns>
		public override Int32 SaveChanges() {
			return this.SaveChangesWithTriggers(acceptAllChangesOnSuccess: true);
		}

		/// <summary>
		///     Saves all changes made in this context to the database.
		/// </summary>
		/// <param name="acceptAllChangesOnSuccess">
		///     Indicates whether <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges" /> is called after the changes have
		///     been sent successfully to the database.
		/// </param>
		/// <remarks>
		///     This method will automatically call <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges" /> to discover any
		///     changes to entity instances before saving to the underlying database. This can be disabled via
		///     <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled" />.
		/// </remarks>
		/// <returns>
		///     The number of state entries written to the database.
		/// </returns>
		public override Int32 SaveChanges(Boolean acceptAllChangesOnSuccess) {
			return this.SaveChangesWithTriggers(acceptAllChangesOnSuccess);
		}

		/// <summary>
		///     Asynchronously saves all changes made in this context to the database.
		/// </summary>
		/// <remarks>
		///     <para>
		///         This method will automatically call <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges" /> to discover any
		///         changes to entity instances before saving to the underlying database. This can be disabled via
		///         <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled" />.
		///     </para>
		///     <para>
		///         Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
		///         that any asynchronous operations have completed before calling another method on this context.
		///     </para>
		/// </remarks>
		/// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous save operation. The task result contains the
		///     number of state entries written to the database.
		/// </returns>
		public override Task<Int32> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken)) {
			return this.SaveChangesWithTriggersAsync(acceptAllChangesOnSuccess: true, cancellationToken: cancellationToken);
		}

		/// <summary>
		///     Asynchronously saves all changes made in this context to the database.
		/// </summary>
		/// <param name="acceptAllChangesOnSuccess">
		///     Indicates whether <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges" /> is called after the changes have
		///     been sent successfully to the database.
		/// </param>
		/// <remarks>
		///     <para>
		///         This method will automatically call <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges" /> to discover any
		///         changes to entity instances before saving to the underlying database. This can be disabled via
		///         <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled" />.
		///     </para>
		///     <para>
		///         Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
		///         that any asynchronous operations have completed before calling another method on this context.
		///     </para>
		/// </remarks>
		/// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous save operation. The task result contains the
		///     number of state entries written to the database.
		/// </returns>
		public override Task<Int32> SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken)) {
			return this.SaveChangesWithTriggersAsync(acceptAllChangesOnSuccess, cancellationToken);
		}

		/// <summary>
		///     <para>
		///         Initializes a new instance of the <see cref="T:EntityFrameworkCore.Triggers.DbContextWithTriggers" /> class. The
		///         <see cref="M:Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)" />
		///         method will be called to configure the database (and other options) to be used for this context.
		///     </para>
		/// </summary>
		protected DbContextWithTriggers() : base() { }

		/// <summary>
		///     <para>
		///         Initializes a new instance of the <see cref="T:EntityFrameworkCore.Triggers.DbContextWithTriggers" /> class using the specified options.
		///         The <see cref="M:Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)" /> method will still be called to allow further
		///         configuration of the options.
		///     </para>
		/// </summary>
		/// <param name="options">The options for this context.</param>
		protected DbContextWithTriggers(DbContextOptions options) : base(options) { }
	}
}
