using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
#else
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
#endif

namespace EntityFramework.Triggers {
	internal static class ExtensionHelpers {
		public static readonly ConditionalWeakTable<ITriggerable, ITriggers> TriggersWeakRefs = new ConditionalWeakTable<ITriggerable, ITriggers>();

		private static ITriggers Triggers(this ITriggerable triggerable) {
			ITriggers triggers;
			TriggersWeakRefs.TryGetValue(triggerable, out triggers);
			return triggers;
		}

		public static ITriggers StaticTriggers(this ITriggerable triggerable) => EntityFramework.Triggers.StaticTriggers.GetStaticTriggers(triggerable.GetType());

		public static AfterActions RaiseTheBeforeEvents(this DbContext dbContext) {
			var entries = dbContext.ChangeTracker.Entries<ITriggerable>().ToArray();
			var afterActions = new AfterActions { Static = new List<Action<DbContext>>(), Instance = new List<Action<DbContext>>() };
			foreach (var entry in entries) {
				var staticTriggers = entry.Entity.StaticTriggers();
				var afterAction = dbContext.RaiseTheBeforeEvent(entry, staticTriggers);
				if (afterAction != null)
					afterActions.Static.Add(afterAction);
			}
			foreach (var entry in entries) {
				var triggers = entry.Entity.Triggers();
				if (triggers == null)
					continue;
				var afterAction = dbContext.RaiseTheBeforeEvent(entry, triggers);
				if (afterAction != null)
					afterActions.Instance.Add(afterAction);
			}
			return afterActions;
		}

#if EF_CORE
		private static Action<DbContext> RaiseTheBeforeEvent(this DbContext dbContext, EntityEntry<ITriggerable> entry, ITriggers triggers) {
#else
		private static Action<DbContext> RaiseTheBeforeEvent(this DbContext dbContext, DbEntityEntry<ITriggerable> entry, ITriggers triggers) {
#endif
			switch (entry.State) {
				case EntityState.Added:
					triggers.OnBeforeInsert(entry.Entity, dbContext);
					if (entry.State == EntityState.Added)
						return context => triggers.OnAfterInsert(entry.Entity, context);
					break;
				case EntityState.Deleted:
					triggers.OnBeforeDelete(entry.Entity, dbContext);
					if (entry.State == EntityState.Deleted)
						return context => triggers.OnAfterDelete(entry.Entity, context);
					break;
				case EntityState.Modified:
					triggers.OnBeforeUpdate(entry.Entity, dbContext);
					if (entry.State == EntityState.Modified)
						return context => triggers.OnAfterUpdate(entry.Entity, context);
					break;
			}
			return null;
		}

		public static void RaiseTheAfterEvents(this DbContext dbContext, AfterActions afterActions) {
			foreach (var afterAction in afterActions.Static)
				afterAction(dbContext);
			foreach (var afterAction in afterActions.Instance)
				afterAction(dbContext);
		}

		public static void RaiseTheFailedEvents(this DbContext dbContext, DbUpdateException dbUpdateException) {
			foreach (var entry in dbUpdateException.Entries)
				(entry.Entity as ITriggerable)?.StaticTriggers()?.RaiseTheFailedEvents(dbContext, entry, dbUpdateException);
			foreach (var entry in dbUpdateException.Entries)
				(entry.Entity as ITriggerable)?.Triggers()?.RaiseTheFailedEvents(dbContext, entry, dbUpdateException);
		}

#if !EF_CORE
		public static void RaiseTheFailedEvents(this DbContext dbContext, DbEntityValidationException dbEntityValidationException) {
			foreach (var dbEntityValidationResult in dbEntityValidationException.EntityValidationErrors)
				(dbEntityValidationResult.Entry.Entity as ITriggerable)?.StaticTriggers()?.RaiseTheFailedEvents(dbContext, dbEntityValidationResult.Entry, dbEntityValidationException);
			foreach (var dbEntityValidationResult in dbEntityValidationException.EntityValidationErrors)
				(dbEntityValidationResult.Entry.Entity as ITriggerable)?.Triggers()?.RaiseTheFailedEvents(dbContext, dbEntityValidationResult.Entry, dbEntityValidationException);
		}
#endif

#if EF_CORE
		private static void RaiseTheFailedEvents(this ITriggers triggers, DbContext dbContext, EntityEntry entry, Exception exception) {
#else
		private static void RaiseTheFailedEvents(this ITriggers triggers, DbContext dbContext, DbEntityEntry entry, Exception exception) {
#endif
			var triggerable = (ITriggerable) entry.Entity;
			switch (entry.State) {
				case EntityState.Added:
					triggers.OnInsertFailed(triggerable, dbContext, exception);
					break;
				case EntityState.Modified:
					triggers.OnUpdateFailed(triggerable, dbContext, exception);
					break;
				case EntityState.Deleted:
					triggers.OnDeleteFailed(triggerable, dbContext, exception);
					break;
			}
		}

		private delegate Int32 SaveChangesDelegateType(DbContext dbContext);

		private static class BaseSaveChangesDelegateCache<TDbContext> where TDbContext : DbContext {
			public static readonly SaveChangesDelegateType SaveChanges = CreateBaseSaveChangesDelegate(typeof(TDbContext));

			private static SaveChangesDelegateType CreateBaseSaveChangesDelegate(Type dbContextType) {
				var dynamicMethod = new DynamicMethod("DbContextBaseSaveChanges", typeof(Int32), new[] { typeof(DbContext) }, typeof(Extensions).Module);
				var ilGenerator = dynamicMethod.GetILGenerator();
				ilGenerator.Emit(OpCodes.Ldarg_0);
				ilGenerator.Emit(OpCodes.Call, dbContextType.BaseType.GetMethod(nameof(DbContext.SaveChanges)));
				ilGenerator.Emit(OpCodes.Ret);
				return (SaveChangesDelegateType) dynamicMethod.CreateDelegate(typeof(SaveChangesDelegateType));
			}
		}

		public static Int32 BaseSaveChanges<TDbContext>(this TDbContext dbContext) where TDbContext : DbContext {
			return BaseSaveChangesDelegateCache<TDbContext>.SaveChanges(dbContext);
		}

#if !NET40
		private delegate Task<Int32> SaveChangesAsyncDelegateType(DbContext dbContext, CancellationToken cancellationToken);

		private static class BaseSaveChangesAsyncDelegateCache<TDbContext> where TDbContext : DbContext {
			public static readonly SaveChangesAsyncDelegateType SaveChangesAsync = CreateBaseSaveChangesAsyncDelegate(typeof(TDbContext));

			private static SaveChangesAsyncDelegateType CreateBaseSaveChangesAsyncDelegate(Type dbContextType) {
				var dynamicMethod = new DynamicMethod("DbContextBaseSaveChangesAsync", typeof(Task<Int32>), new[] { typeof(DbContext), typeof(CancellationToken) }, typeof(Extensions).Module);
				var ilGenerator = dynamicMethod.GetILGenerator();
				ilGenerator.Emit(OpCodes.Ldarg_0);
				ilGenerator.Emit(OpCodes.Ldarg_1);
				ilGenerator.Emit(OpCodes.Call, dbContextType.BaseType.GetMethod(nameof(DbContext.SaveChangesAsync), new[] { typeof(CancellationToken) }));
				ilGenerator.Emit(OpCodes.Ret);
				return (SaveChangesAsyncDelegateType) dynamicMethod.CreateDelegate(typeof(SaveChangesAsyncDelegateType));
			}
		}

		public static Task<Int32> BaseSaveChangesAsync<TDbContext>(this TDbContext dbContext, CancellationToken cancellationToken) where TDbContext : DbContext {
			return BaseSaveChangesAsyncDelegateCache<TDbContext>.SaveChangesAsync(dbContext, cancellationToken);
		}
#endif
	}
}