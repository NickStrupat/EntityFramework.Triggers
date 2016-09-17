using System;
using System.Threading;
using System.Threading.Tasks;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers {
#else
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
namespace EntityFramework.Triggers {
#endif
	public static class Extensions {
		/// <summary>
		/// Saves all changes made in this context to the underlying database, firing trigger events accordingly.
		/// </summary>
		/// <param name="dbContext"></param>
#if EF_CORE
		/// <param name="acceptAllChangesOnSuccess">
		///     Indicates whether <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges" /> is called after the changes have
		///     been sent successfully to the database.
		/// </param>
#endif
		/// <example>this.SaveChangesWithTriggers();</example>
		/// <returns>The number of objects written to the underlying database.</returns>
#if EF_CORE
		public static Int32 SaveChangesWithTriggers(this DbContext dbContext, Boolean acceptAllChangesOnSuccess = true) {
#else
		public static Int32 SaveChangesWithTriggers(this DbContext dbContext) {
#endif
			if (dbContext == null)
				throw new ArgumentNullException(nameof(dbContext));
			var invoker = TriggerInvokers.Get(dbContext.GetType());
			try {
				var afterActions = invoker.RaiseTheBeforeEvents(dbContext);
#if EF_CORE
				var result = invoker.BaseSaveChanges(dbContext, acceptAllChangesOnSuccess);
#else
				var result = invoker.BaseSaveChanges(dbContext);
#endif
				invoker.RaiseTheAfterEvents(dbContext, afterActions);
				return result;
			}
			catch (DbUpdateException dbUpdateException) when (ExceptionFilterAction(() => invoker.RaiseTheFailedEvents(dbContext, dbUpdateException))) {
				throw new InvalidOperationException(CaughtExceptionMessage, dbUpdateException);
			}
#if !EF_CORE
			catch (DbEntityValidationException dbEntityValidationException) when (ExceptionFilterAction(() => invoker.RaiseTheFailedEvents(dbContext, dbEntityValidationException))) {
				throw new InvalidOperationException(CaughtExceptionMessage, dbEntityValidationException);
			}
#endif
		}

#if !NET40
		/// <summary>
		/// Asynchronously saves all changes made in this context to the underlying database, firing trigger events accordingly.
		/// </summary>
		/// <param name="dbContext"></param>
#if EF_CORE
		/// <param name="acceptAllChangesOnSuccess">
		///     Indicates whether <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges" /> is called after the changes have
		///     been sent successfully to the database.
		/// </param>
#endif
		/// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <example>this.SaveChangesWithTriggersAsync();</example>
		/// <returns>A task that represents the asynchronous save operation. The task result contains the number of objects written to the underlying database.</returns>
#if EF_CORE
		public static async Task<Int32> SaveChangesWithTriggersAsync(this DbContext dbContext, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken)) {
#else
		public static async Task<Int32> SaveChangesWithTriggersAsync(this DbContext dbContext, CancellationToken cancellationToken = default(CancellationToken)) {
#endif
			if (dbContext == null)
				throw new ArgumentNullException(nameof(dbContext));
			var invoker = TriggerInvokers.Get(dbContext.GetType());
			try {
				var afterActions = invoker.RaiseTheBeforeEvents(dbContext);
#if EF_CORE
				var result = await invoker.BaseSaveChangesAsync(dbContext, acceptAllChangesOnSuccess, cancellationToken);
#else
				var result = await invoker.BaseSaveChangesAsync(dbContext, cancellationToken);
#endif
				invoker.RaiseTheAfterEvents(dbContext, afterActions);
				return result;
			}
			catch (DbUpdateException dbUpdateException) when (ExceptionFilterAction(() => invoker.RaiseTheFailedEvents(dbContext, dbUpdateException))) {
				throw new InvalidOperationException(CaughtExceptionMessage, dbUpdateException);
			}
#if !EF_CORE
			catch (DbEntityValidationException dbEntityValidationException) when (ExceptionFilterAction(() => invoker.RaiseTheFailedEvents(dbContext, dbEntityValidationException))) {
				throw new InvalidOperationException(CaughtExceptionMessage, dbEntityValidationException);
			}
#endif
		}

#if EF_CORE
		public static Task<Int32> SaveChangesWithTriggersAsync(this DbContext dbContext, CancellationToken cancellationToken = default(CancellationToken)) {
			return dbContext.SaveChangesWithTriggersAsync(true, cancellationToken);
		}
#endif
#endif

		private const String CaughtExceptionMessage = "An exception was caught which instead should have been observed in the exception catch filter and not caught.";

		private static Boolean ExceptionFilterAction(Action action) {
			action();
			return false;
		}
	}
}