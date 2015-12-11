using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFramework.Triggers {
	public static class Extensions {
		/// <summary>
		/// Retrieve the <see cref="T:Triggers`1{TTriggerable}"/> object that contains the trigger events for this <see cref="ITriggerable"/>
		/// </summary>
		/// <typeparam name="TTriggerable"></typeparam>
		/// <param name="triggerable"></param>
		/// <returns></returns>
		public static ITriggers<TTriggerable> Triggers<TTriggerable>(this TTriggerable triggerable) where TTriggerable : class, ITriggerable {
			var triggers = ExtensionHelpers.TriggersWeakRefs.GetValue(triggerable, EntityFramework.Triggers.Triggers.Create);
			return (ITriggers<TTriggerable>) triggers;
		}

		private static readonly ConditionalWeakTable<ITriggerable, ITriggers> TypedContextTriggersWeakRefs = new ConditionalWeakTable<ITriggerable, ITriggers>();

		public static ITriggers<TTriggerable, TDbContext> Triggers<TTriggerable, TDbContext>(this TTriggerable triggerable) where TTriggerable : class, ITriggerable where TDbContext : DbContext {
			var triggers = TypedContextTriggersWeakRefs.GetValue(triggerable, EntityFramework.Triggers.Triggers.Create<TDbContext	);
			return (ITriggers<TTriggerable, TDbContext>) triggers;
			//var triggers = triggerable.Triggers();
			//return new Triggers<TTriggerable, TDbContext>((Triggers<TTriggerable>) triggers);
		}

		/// <summary>
		/// Saves all changes made in this context to the underlying database, firing trigger events accordingly.
		/// </summary>
		/// <param name="dbContext"></param>
		/// <example>this.SaveChangesWithTriggers();</example>
		/// <returns>The number of objects written to the underlying database.</returns>
		public static Int32 SaveChangesWithTriggers<TDbContext>(this TDbContext dbContext) where TDbContext : DbContext {
			if (dbContext == null)
				throw new ArgumentNullException(nameof(dbContext));
			try {
				var afterActions = dbContext.RaiseTheBeforeEvents();
				var result = dbContext.BaseSaveChanges();
				dbContext.RaiseTheAfterEvents(afterActions);
				return result;
			}
			catch (DbUpdateException dbUpdateException) when (ExceptionFilterAction(() => dbContext.RaiseTheFailedEvents(dbUpdateException))) {
				throw new InvalidOperationException(CaughtExceptionMessage, dbUpdateException);
			}
			catch (DbEntityValidationException dbEntityValidationException) when (ExceptionFilterAction(() => dbContext.RaiseTheFailedEvents(dbEntityValidationException))) {
				throw new InvalidOperationException(CaughtExceptionMessage, dbEntityValidationException);
			}
		}

		private const String CaughtExceptionMessage = "An exception was caught which instead should have been observed in the exception catch filter.";

		private static Boolean ExceptionFilterAction(Action action) {
			action();
			return false;
		}

#if !NET40
		/// <summary>
		/// Asynchronously saves all changes made in this context to the underlying database, firing trigger events accordingly.
		/// </summary>
		/// <param name="dbContext"></param>
		/// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <example>this.SaveChangesWithTriggersAsync();</example>
		/// <returns>A task that represents the asynchronous save operation. The task result contains the number of objects written to the underlying database.</returns>
		public static async Task<Int32> SaveChangesWithTriggersAsync<TDbContext>(this TDbContext dbContext, CancellationToken cancellationToken) where TDbContext : DbContext {
			if (dbContext == null)
				throw new ArgumentNullException(nameof(dbContext));
			try {
				var afterActions = dbContext.RaiseTheBeforeEvents();
				var result = await dbContext.BaseSaveChangesAsync(cancellationToken);
				dbContext.RaiseTheAfterEvents(afterActions);
				return result;
			}
			catch (DbUpdateException dbUpdateException) when (ExceptionFilterAction(() => dbContext.RaiseTheFailedEvents(dbUpdateException))) {
				throw new InvalidOperationException(CaughtExceptionMessage, dbUpdateException);
			}
			catch (DbEntityValidationException dbEntityValidationException) when (ExceptionFilterAction(() => dbContext.RaiseTheFailedEvents(dbEntityValidationException))) {
				throw new InvalidOperationException(CaughtExceptionMessage, dbEntityValidationException);
			}
		}
#endif
	}
}