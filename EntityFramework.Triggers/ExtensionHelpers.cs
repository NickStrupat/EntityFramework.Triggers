using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFramework.Triggers {
    internal static class ExtensionHelpers {
        public static readonly ConditionalWeakTable<ITriggerable, ITriggers> TriggersWeakRefs = new ConditionalWeakTable<ITriggerable, ITriggers>();

        private static ITriggers Triggers(this ITriggerable triggerable) {
            ITriggers triggers;
            TriggersWeakRefs.TryGetValue(triggerable, out triggers);
            return triggers;
        }

        public static IEnumerable<Action<DbContext>> RaiseTheBeforeEvents(this DbContext dbContext) {
            var afterActions = new List<Action<DbContext>>();
            foreach (var entry in dbContext.ChangeTracker.Entries<ITriggerable>()) {
                var triggers = entry.Entity.Triggers();
                if (triggers == null)
                    continue;
                switch (entry.State) {
                    case EntityState.Added:
                        triggers.OnBeforeInsert(entry.Entity, dbContext);
                        if (entry.State == EntityState.Added)
                            afterActions.Add(context => triggers.OnAfterInsert(entry.Entity, context));
                        break;
                    case EntityState.Deleted:
                        triggers.OnBeforeDelete(entry.Entity, dbContext);
                        if (entry.State == EntityState.Deleted)
                            afterActions.Add(context => triggers.OnAfterDelete(entry.Entity, context));
                        break;
                    case EntityState.Modified:
                        triggers.OnBeforeUpdate(entry.Entity, dbContext);
                        if (entry.State == EntityState.Modified)
                            afterActions.Add(context => triggers.OnAfterUpdate(entry.Entity, context));
                        break;
                }
            }
            return afterActions;
        }

        public static void RaiseTheAfterEvents(this DbContext dbContext, IEnumerable<Action<DbContext>> afterActions) {
            foreach (var afterAction in afterActions)
                afterAction(dbContext);
        }

        public static void RaiseTheFailedEvents(this DbContext dbContext, DbUpdateException dbUpdateException) {
            foreach (var entry in dbUpdateException.Entries.Where(x => x.Entity is ITriggerable))
                RaiseTheFailedEvents(dbContext, entry, dbUpdateException);
        }

        public static void RaiseTheFailedEvents(this DbContext dbContext, DbEntityValidationException dbEntityValidationException) {
            foreach (var dbEntityValidationResult in dbEntityValidationException.EntityValidationErrors.Where(x => x.Entry.Entity is ITriggerable))
                RaiseTheFailedEvents(dbContext, dbEntityValidationResult.Entry, dbEntityValidationException);
        }

        private static void RaiseTheFailedEvents(this DbContext dbContext, DbEntityEntry entry, Exception exception) {
            var triggerable = (ITriggerable) entry.Entity;
            var triggers = triggerable.Triggers();
            if (triggers == null)
                return;
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

        private static readonly ConcurrentDictionary<Type, SaveChangesDelegateType> baseSaveChangesDelegateCache = new ConcurrentDictionary<Type, SaveChangesDelegateType>();

        public static Int32 BaseSaveChanges(this DbContext dbContext) {
            var baseSaveChangesDelegate = baseSaveChangesDelegateCache.GetOrAdd(dbContext.GetType(), CreateBaseSaveChangesDelegate);
            return baseSaveChangesDelegate(dbContext);
        }

        private static SaveChangesDelegateType CreateBaseSaveChangesDelegate(Type dbContextType) {
            var dynamicMethod = new DynamicMethod("DbContextBaseSaveChanges", typeof(Int32), new[] { typeof(DbContext) }, typeof(Extensions).Module);
            var ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Call, dbContextType.BaseType.GetMethod(nameof(DbContext.SaveChanges)));
            ilGenerator.Emit(OpCodes.Ret);
            return (SaveChangesDelegateType)dynamicMethod.CreateDelegate(typeof(SaveChangesDelegateType));
        }

#if !NET40
        private delegate Task<Int32> SaveChangesAsyncDelegateType(DbContext dbContext, CancellationToken cancellationToken);

        private static readonly ConcurrentDictionary<Type, SaveChangesAsyncDelegateType> baseSaveChangesAsyncDelegateCache = new ConcurrentDictionary<Type, SaveChangesAsyncDelegateType>();

        public static Task<Int32> BaseSaveChangesAsync(this DbContext dbContext, CancellationToken cancellationToken) {
            var baseSaveChangesAsyncDelegate = baseSaveChangesAsyncDelegateCache.GetOrAdd(dbContext.GetType(), CreateBaseSaveChangesAsyncDelegate);
            return baseSaveChangesAsyncDelegate(dbContext, cancellationToken);
        }

        private static SaveChangesAsyncDelegateType CreateBaseSaveChangesAsyncDelegate(Type dbContextType) {
            var dynamicMethod = new DynamicMethod("DbContextBaseSaveChangesAsync", typeof(Task<Int32>), new[] { typeof(DbContext), typeof(CancellationToken) }, typeof(Extensions).Module);
            var ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Call, dbContextType.BaseType.GetMethod(nameof(DbContext.SaveChangesAsync), new[] { typeof(CancellationToken) }));
            ilGenerator.Emit(OpCodes.Ret);
            return (SaveChangesAsyncDelegateType)dynamicMethod.CreateDelegate(typeof(SaveChangesAsyncDelegateType));
        }
#endif
    }
}