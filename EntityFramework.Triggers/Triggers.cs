using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq.Expressions;

namespace EntityFramework.Triggers {
    internal static class Triggers {
        private static readonly ConcurrentDictionary<Type, Func<ITriggers>> triggersConstructorCache = new ConcurrentDictionary<Type, Func<ITriggers>>();
        
        internal static ITriggers Create(ITriggerable triggerable) {
            var triggersConstructor = triggersConstructorCache.GetOrAdd(triggerable.GetType(), TriggersConstructorFactory);
            return triggersConstructor();
        }

        private static Func<ITriggers> TriggersConstructorFactory(Type x) {
            return Expression.Lambda<Func<ITriggers>>(Expression.New(typeof (Triggers<>).MakeGenericType(x))).Compile();
        }
    }

    internal sealed class Triggers<TTriggerable> : ITriggers<TTriggerable>, ITriggers where TTriggerable : class, ITriggerable {
        private Triggers() {}

        #region Entry implementations
        private class Entry : IEntry<TTriggerable> {
            public TTriggerable Entity { get; internal set; }
            public DbContext Context { get; internal set; }
        }

        private class FailedEntry : Entry, IFailedEntry<TTriggerable> {
            public Exception Exception { get; internal set; }
        }

        private abstract class BeforeEntry : Entry, IBeforeEntry<TTriggerable> {
            public virtual void Cancel() {
                Context.Entry(Entity).State = EntityState.Unchanged;
            }
        }

        private class InsertingEntry : BeforeEntry {}

        private class UpdatingEntry : BeforeEntry {}

        private class DeletingEntry : BeforeEntry {
            public override void Cancel() {
                Context.Entry(Entity).State = EntityState.Modified;
            }
        }
        #endregion
        #region Event helpers
        private static void AddEventHandler<TIEntry>(List<Action<TIEntry>> eventHandlers, Action<TIEntry> value) {
            lock (eventHandlers)
                eventHandlers.Add(value);
        }

        private static void RemoveEventHandler<TIEntry>(List<Action<TIEntry>> eventHandlers, Action<TIEntry> value) {
            lock (eventHandlers)
                eventHandlers.Remove(value);
        }

        private static void RaiseEventHandlers<TIEntry>(List<Action<TIEntry>> eventHandlers, TIEntry entry) {
            foreach (var eventHandler in eventHandlers)
                eventHandler(entry);
        }
        #endregion
        #region Event implementations
        private readonly List<Action<IBeforeEntry<TTriggerable>>> insertingEventHandlers = new List<Action<IBeforeEntry<TTriggerable>>>();
        private readonly List<Action<IBeforeEntry<TTriggerable>>> updatingEventHandlers = new List<Action<IBeforeEntry<TTriggerable>>>();
        private readonly List<Action<IBeforeEntry<TTriggerable>>> deletingEventHandlers = new List<Action<IBeforeEntry<TTriggerable>>>();
        private readonly List<Action<IFailedEntry<TTriggerable>>> insertFailedEventHandlers = new List<Action<IFailedEntry<TTriggerable>>>();
        private readonly List<Action<IFailedEntry<TTriggerable>>> updateFailedEventHandlers = new List<Action<IFailedEntry<TTriggerable>>>();
        private readonly List<Action<IFailedEntry<TTriggerable>>> deleteFailedEventHandlers = new List<Action<IFailedEntry<TTriggerable>>>();
        private readonly List<Action<IEntry<TTriggerable>>> insertedEventHandlers = new List<Action<IEntry<TTriggerable>>>();
        private readonly List<Action<IEntry<TTriggerable>>> updatedEventHandlers = new List<Action<IEntry<TTriggerable>>>();
        private readonly List<Action<IEntry<TTriggerable>>> deletedEventHandlers = new List<Action<IEntry<TTriggerable>>>();

        event Action<IBeforeEntry<TTriggerable>> ITriggers<TTriggerable>.Inserting {
            add { AddEventHandler(insertingEventHandlers, value); }
            remove { RemoveEventHandler(insertingEventHandlers, value); }
        }

        event Action<IBeforeEntry<TTriggerable>> ITriggers<TTriggerable>.Updating {
            add { AddEventHandler(updatingEventHandlers, value); }
            remove { RemoveEventHandler(updatingEventHandlers, value); }
        }

        event Action<IBeforeEntry<TTriggerable>> ITriggers<TTriggerable>.Deleting {
            add { AddEventHandler(deletingEventHandlers, value); }
            remove { RemoveEventHandler(deletingEventHandlers, value); }
        }

        event Action<IFailedEntry<TTriggerable>> ITriggers<TTriggerable>.InsertFailed {
            add { AddEventHandler(insertFailedEventHandlers, value); }
            remove { RemoveEventHandler(insertFailedEventHandlers, value); }
        }

        event Action<IFailedEntry<TTriggerable>> ITriggers<TTriggerable>.UpdateFailed {
            add { AddEventHandler(updateFailedEventHandlers, value); }
            remove { RemoveEventHandler(updateFailedEventHandlers, value); }
        }

        event Action<IFailedEntry<TTriggerable>> ITriggers<TTriggerable>.DeleteFailed {
            add { AddEventHandler(deleteFailedEventHandlers, value); }
            remove { RemoveEventHandler(deleteFailedEventHandlers, value); }
        }

        event Action<IEntry<TTriggerable>> ITriggers<TTriggerable>.Inserted {
            add { AddEventHandler(insertedEventHandlers, value); }
            remove { RemoveEventHandler(insertedEventHandlers, value); }
        }

        event Action<IEntry<TTriggerable>> ITriggers<TTriggerable>.Updated {
            add { AddEventHandler(updatedEventHandlers, value); }
            remove { RemoveEventHandler(updatedEventHandlers, value); }
        }

        event Action<IEntry<TTriggerable>> ITriggers<TTriggerable>.Deleted {
            add { AddEventHandler(deletedEventHandlers, value); }
            remove { RemoveEventHandler(deletedEventHandlers, value); }
        }
        #endregion
        #region Raise events
        void ITriggers.OnBeforeInsert(ITriggerable triggerable, DbContext dbContext) {
            RaiseEventHandlers(insertingEventHandlers, new InsertingEntry { Entity = (TTriggerable)triggerable, Context = dbContext });
        }

        void ITriggers.OnBeforeUpdate(ITriggerable triggerable, DbContext dbContext) {
            RaiseEventHandlers(updatingEventHandlers, new UpdatingEntry { Entity = (TTriggerable)triggerable, Context = dbContext });
        }

        void ITriggers.OnBeforeDelete(ITriggerable triggerable, DbContext dbContext) {
            RaiseEventHandlers(deletingEventHandlers, new DeletingEntry { Entity = (TTriggerable)triggerable, Context = dbContext });
        }

        void ITriggers.OnInsertFailed(ITriggerable triggerable, DbContext dbContext, Exception exception) {
            RaiseEventHandlers(insertFailedEventHandlers, new FailedEntry { Entity = (TTriggerable)triggerable, Context = dbContext, Exception = exception });
        }

        void ITriggers.OnUpdateFailed(ITriggerable triggerable, DbContext dbContext, Exception exception) {
            RaiseEventHandlers(updateFailedEventHandlers, new FailedEntry { Entity = (TTriggerable)triggerable, Context = dbContext, Exception = exception });
        }

        void ITriggers.OnDeleteFailed(ITriggerable triggerable, DbContext dbContext, Exception exception) {
            RaiseEventHandlers(deleteFailedEventHandlers, new FailedEntry { Entity = (TTriggerable)triggerable, Context = dbContext, Exception = exception });
        }

        void ITriggers.OnAfterInsert(ITriggerable triggerable, DbContext dbContext) {
            RaiseEventHandlers(insertedEventHandlers, new Entry { Entity = (TTriggerable)triggerable, Context = dbContext });
        }

        void ITriggers.OnAfterUpdate(ITriggerable triggerable, DbContext dbContext) {
            RaiseEventHandlers(updatedEventHandlers, new Entry { Entity = (TTriggerable)triggerable, Context = dbContext });
        }

        void ITriggers.OnAfterDelete(ITriggerable triggerable, DbContext dbContext) {
            RaiseEventHandlers(deletedEventHandlers, new Entry { Entity = (TTriggerable)triggerable, Context = dbContext });
        }
        #endregion
    }
}