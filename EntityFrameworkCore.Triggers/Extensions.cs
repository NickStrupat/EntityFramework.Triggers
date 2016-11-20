using System;
using System.Reflection;
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
		/// Saves all changes made in this context to the underlying database, firing trigger events accordingly. Only call this within your DbContext class.
		/// </summary>
		/// <param name="dbContext"></param>
		/// <param name="baseSaveChanges">Always pass base.SaveChanges</param>
#if EF_CORE
		/// <param name="acceptAllChangesOnSuccess">
		///     Indicates whether <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges" /> is called after the changes have
		///     been sent successfully to the database.
		/// </param>
		/// <example>this.SaveChangesWithTriggers(base.SaveChanges, true);</example>
#else
		/// <example>this.SaveChangesWithTriggers(base.SaveChanges);</example>
#endif
		/// <returns>The number of objects written to the underlying database.</returns>
#if EF_CORE
		public static Int32 SaveChangesWithTriggers(this DbContext dbContext, Func<Boolean, Int32> baseSaveChanges, Boolean acceptAllChangesOnSuccess = true) {
#else
		public static Int32 SaveChangesWithTriggers(this DbContext dbContext, Func<Int32> baseSaveChanges) {
#endif
			if (dbContext == null)
				throw new ArgumentNullException(nameof(dbContext));
			var dbContextType = dbContext.GetType();
			var invoker = TriggerInvokers.Get(dbContextType);
			var swallow = false;
			try {
				var afterActions = invoker.RaiseTheBeforeEvents(dbContext);
#if EF_CORE
				var result = baseSaveChanges(acceptAllChangesOnSuccess);
#else
				var result = baseSaveChanges();
#endif
				invoker.RaiseTheAfterEvents(dbContext, afterActions);
				return result;
			}
			catch (DbUpdateException ex) when (invoker.RaiseTheFailedEvents(dbContext, ex, ref swallow)) {
			}
#if !EF_CORE
			catch (DbEntityValidationException ex) when (invoker.RaiseTheFailedEvents(dbContext, ex, ref swallow)) {
			}
#endif
			catch (Exception ex) when(invoker.RaiseTheFailedEvents(dbContext, ex, ref swallow)) {
			}
			return 0;
		}

#if !NET40
		/// <summary>
		/// Asynchronously saves all changes made in this context to the underlying database, firing trigger events accordingly.
		/// </summary>
		/// <param name="dbContext"></param>
		/// <param name="baseSaveChangesAsync">Always pass base.SaveChangesAsync</param>
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
		public static async Task<Int32> SaveChangesWithTriggersAsync(this DbContext dbContext, Func<Boolean, CancellationToken, Task<Int32>> baseSaveChangesAsync, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken)) {
#else
		public static async Task<Int32> SaveChangesWithTriggersAsync(this DbContext dbContext, Func<CancellationToken, Task<Int32>> baseSaveChangesAsync, CancellationToken cancellationToken = default(CancellationToken)) {
#endif
			if (dbContext == null)
				throw new ArgumentNullException(nameof(dbContext));
			var invoker = TriggerInvokers.Get(dbContext.GetType());
			var swallow = false;
			try {
				var afterActions = invoker.RaiseTheBeforeEvents(dbContext);
#if EF_CORE
				var result = await baseSaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
#else
				var result = await baseSaveChangesAsync(cancellationToken);
#endif
				invoker.RaiseTheAfterEvents(dbContext, afterActions);
				return result;
			}
			catch (DbUpdateException ex) when(invoker.RaiseTheFailedEvents(dbContext, ex, ref swallow)) {
			}
#if !EF_CORE
			catch (DbEntityValidationException ex) when(invoker.RaiseTheFailedEvents(dbContext, ex, ref swallow)) {
			}
#endif
			catch (Exception ex) when (invoker.RaiseTheFailedEvents(dbContext, ex, ref swallow)) {
			}
			return 0;
		}

#if EF_CORE
		public static Task<Int32> SaveChangesWithTriggersAsync(this DbContext dbContext, Func<Boolean, CancellationToken, Task<Int32>> baseSaveChangesAsync, CancellationToken cancellationToken = default(CancellationToken)) {
			return dbContext.SaveChangesWithTriggersAsync(baseSaveChangesAsync, true, cancellationToken);
		}
#endif
#endif
	}
}