using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Triggers {
	public static class Extensions {
		/// <summary>
		/// Saves all changes made in this context to the underlying database, firing trigger events accordingly.
		/// </summary>
		/// <param name="dbContext"></param>
		/// <param name="acceptAllChangesOnSuccess">
		///     Indicates whether <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges" /> is called after the changes have
		///     been sent successfully to the database.
		/// </param>
		/// <example>this.SaveChangesWithTriggers();</example>
		/// <returns>The number of objects written to the underlying database.</returns>
		public static Int32 SaveChangesWithTriggers(this DbContext dbContext, Boolean acceptAllChangesOnSuccess = true) {
			if (dbContext == null)
				throw new ArgumentNullException(nameof(dbContext));
			var invoker = TriggerInvokers.Get(dbContext.GetType());
			try {
				var afterActions = invoker.RaiseTheBeforeEvents(dbContext);
				var result = invoker.BaseSaveChanges(dbContext, acceptAllChangesOnSuccess);
				invoker.RaiseTheAfterEvents(dbContext, afterActions);
				return result;
			}
			catch (DbUpdateException dbUpdateException) when (ExceptionFilterAction(() => invoker.RaiseTheFailedEvents(dbContext, dbUpdateException))) {
				throw new InvalidOperationException(CaughtExceptionMessage, dbUpdateException);
			}
		}

		/// <summary>
		/// Asynchronously saves all changes made in this context to the underlying database, firing trigger events accordingly.
		/// </summary>
		/// <param name="dbContext"></param>
		/// <param name="acceptAllChangesOnSuccess">
		///     Indicates whether <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges" /> is called after the changes have
		///     been sent successfully to the database.
		/// </param>
		/// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <example>this.SaveChangesWithTriggersAsync();</example>
		/// <returns>A task that represents the asynchronous save operation. The task result contains the number of objects written to the underlying database.</returns>
		public static async Task<Int32> SaveChangesWithTriggersAsync(this DbContext dbContext, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken) {
			if (dbContext == null)
				throw new ArgumentNullException(nameof(dbContext));
			var invoker = TriggerInvokers.Get(dbContext.GetType());
			try {
				var afterActions = invoker.RaiseTheBeforeEvents(dbContext);
				var result = await invoker.BaseSaveChangesAsync(dbContext, acceptAllChangesOnSuccess, cancellationToken);
				invoker.RaiseTheAfterEvents(dbContext, afterActions);
				return result;
			}
			catch (DbUpdateException dbUpdateException) when (ExceptionFilterAction(() => invoker.RaiseTheFailedEvents(dbContext, dbUpdateException))) {
				throw new InvalidOperationException(CaughtExceptionMessage, dbUpdateException);
			}
		}

		public static Task<Int32> SaveChangesWithTriggersAsync(this DbContext dbContext, CancellationToken cancellationToken) {
			return dbContext.SaveChangesWithTriggersAsync(true, cancellationToken);
		}

		private const String CaughtExceptionMessage = "An exception was caught which instead should have been observed in the exception catch filter and not caught.";

		private static Boolean ExceptionFilterAction(Action action) {
			action();
			return false;
		}
	}
}