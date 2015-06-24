using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
            var triggers = TriggersWeakRefs.GetValue(triggerable, EntityFramework.Triggers.Triggers.Create);
            return (ITriggers<TTriggerable>) triggers;
        }

        private static readonly ConditionalWeakTable<ITriggerable, ITriggers> TriggersWeakRefs = new ConditionalWeakTable<ITriggerable, ITriggers>();

        private static ITriggers Triggers(this ITriggerable triggerable) {
            ITriggers triggers;
            TriggersWeakRefs.TryGetValue(triggerable, out triggers);
            return triggers;
        }

        private static IEnumerable<Action<DbContext>> RaiseTheBeforeEvents(this DbContext dbContext) {
            var afterActions = new List<Action<DbContext>>();
            foreach (var entry in dbContext.ChangeTracker.Entries<ITriggerable>()) {
                var triggers = entry.Entity.Triggers();
                if (triggers == null)
                    continue;
                var entry1 = entry;
                switch (entry.State) {
                    case EntityState.Added:
                        triggers.OnBeforeInsert(entry.Entity, dbContext);
                        if (entry.State == EntityState.Added)
                            afterActions.Add(context => triggers.OnAfterInsert(entry1.Entity, context));
                        break;
                    case EntityState.Deleted:
                        triggers.OnBeforeDelete(entry.Entity, dbContext);
                        if (entry.State == EntityState.Deleted)
                            afterActions.Add(context => triggers.OnAfterDelete(entry1.Entity, context));
                        break;
                    case EntityState.Modified:
                        triggers.OnBeforeUpdate(entry.Entity, dbContext);
                        if (entry.State == EntityState.Modified)
                            afterActions.Add(context => triggers.OnAfterUpdate(entry1.Entity, context));
                        break;
                }
            }
            return afterActions;
        }

        private static void RaiseTheAfterEvents(this DbContext dbContext, IEnumerable<Action<DbContext>> afterActions) {
            foreach (var afterAction in afterActions)
                afterAction(dbContext);
        }

        private static void RaiseTheFailedEvents(this DbContext dbContext, Exception exception) {
            var dbUpdateException = exception as DbUpdateException;
            var dbEntityValidationException = exception as DbEntityValidationException;
            if (dbUpdateException != null)
                foreach (var entry in dbUpdateException.Entries.Where(x => x.Entity is ITriggerable))
                    RaiseTheFailedEvents(dbContext, entry, dbUpdateException);
            else if (dbEntityValidationException != null)
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

        private static readonly Lazy<SaveChangesDelegateType> dbContextBaseSaveChangesFunc = new Lazy<SaveChangesDelegateType>(DbContextBaseSaveChangesFuncFactory);

        private static SaveChangesDelegateType DbContextBaseSaveChangesFuncFactory() {
            var dynamicMethod = new DynamicMethod("DbContextBaseSaveChanges", typeof (Int32), new[] {typeof (DbContext)}, typeof (Extensions).Module);
            var ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Call, typeof(DbContext).GetMethod("SaveChanges"));
            ilGenerator.Emit(OpCodes.Ret);
            return (SaveChangesDelegateType) dynamicMethod.CreateDelegate(typeof (SaveChangesDelegateType));
        }

        /// <summary>
        /// Saves all changes made in this context to the underlying database, firing trigger events accordingly.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="baseSaveChanges">A delegate to base.SaveChanges(). Always pass `base.SaveChanges`.</param>
        /// <example>this.SaveChangesWithTriggers(base.SaveChanges);</example>
        /// <returns>The number of objects written to the underlying database.</returns>
        [Obsolete("base.SaveChanges is no longer needed.")]
        public static Int32 SaveChangesWithTriggers(this DbContext dbContext, Func<Int32> baseSaveChanges) {
            return dbContext.SaveChangesWithTriggers();
        }

        /// <summary>
        /// Saves all changes made in this context to the underlying database, firing trigger events accordingly.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <example>this.SaveChangesWithTriggers(base.SaveChanges);</example>
        /// <returns>The number of objects written to the underlying database.</returns>
        public static Int32 SaveChangesWithTriggers(this DbContext dbContext) {
            try {
                var afterActions = dbContext.RaiseTheBeforeEvents();
                var result = dbContextBaseSaveChangesFunc.Value(dbContext);
                dbContext.RaiseTheAfterEvents(afterActions);
                return result;
            }
            catch (Exception exception) {
                dbContext.RaiseTheFailedEvents(exception);
                throw;
            }
        }

#if !NET40
        private delegate Task<Int32> SaveChangesAsyncDelegateType(DbContext dbContext, CancellationToken cancellationToken);

        private static readonly Lazy<SaveChangesAsyncDelegateType> dbContextBaseSaveChangesAsyncFunc = new Lazy<SaveChangesAsyncDelegateType>(DbContextBaseSaveChangesAsyncFuncFactory);

        private static SaveChangesAsyncDelegateType DbContextBaseSaveChangesAsyncFuncFactory() {
            var dynamicMethod = new DynamicMethod("DbContextBaseSaveChangesAsync", typeof (Task<Int32>), new[] {typeof (DbContext), typeof (CancellationToken)}, typeof (Extensions).Module);
            var ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Call, typeof(DbContext).GetMethod("SaveChangesAsync", new[] { typeof(CancellationToken) }));
            ilGenerator.Emit(OpCodes.Ret);
            return (SaveChangesAsyncDelegateType) dynamicMethod.CreateDelegate(typeof (SaveChangesAsyncDelegateType));
        }

        /// <summary>
        /// Asynchronously saves all changes made in this context to the underlying database, firing trigger events accordingly.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="baseSaveChangesAsync">A delegate to base.SaveChangesAsync(). Always pass `base.SaveChangesAsync`.</param>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <example>this.SaveChangesWithTriggersAsync(base.SaveChangesAsync);</example>
        /// <returns>A task that represents the asynchronous save operation. The task result contains the number of objects written to the underlying database.</returns>
        [Obsolete]
        public static Task<Int32> SaveChangesWithTriggersAsync(this DbContext dbContext, Func<CancellationToken, Task<Int32>> baseSaveChangesAsync, CancellationToken cancellationToken) {
            return dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously saves all changes made in this context to the underlying database, firing trigger events accordingly.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <example>this.SaveChangesWithTriggersAsync();</example>
        /// <returns>A task that represents the asynchronous save operation. The task result contains the number of objects written to the underlying database.</returns>
        public static async Task<Int32> SaveChangesWithTriggersAsync(this DbContext dbContext, CancellationToken cancellationToken) {
            try {
                var afterActions = dbContext.RaiseTheBeforeEvents();
                var result = await dbContextBaseSaveChangesAsyncFunc.Value(dbContext, cancellationToken);
                dbContext.RaiseTheAfterEvents(afterActions);
                return result;
            }
            catch (Exception exception) {
                dbContext.RaiseTheFailedEvents(exception);
                throw;
            }
        }
#endif
    }
}