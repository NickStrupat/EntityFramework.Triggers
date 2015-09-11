using System;
using System.Collections.Concurrent;
using System.Data.Entity;

namespace EntityFramework.Triggers {
	internal static class StaticTriggers {
		public static ITriggers GetStaticTriggers(Type triggerableType) => staticTriggersCache.GetOrAdd(triggerableType, StaticTriggersFactory);
		private static readonly ConcurrentDictionary<Type, ITriggers> staticTriggersCache = new ConcurrentDictionary<Type, ITriggers>();
		private static ITriggers StaticTriggersFactory(Type type) => (ITriggers)Activator.CreateInstance(typeof(StaticTriggersImplementation<>).MakeGenericType(type));

		private class StaticTriggersImplementation<TTriggerable> : ITriggers where TTriggerable : class, ITriggerable {
			private static readonly Type baseType = typeof (TTriggerable).BaseType;
			private static readonly Boolean isBaseTypeTriggerable = typeof (ITriggerable).IsAssignableFrom(baseType);
			private static readonly ITriggers baseTriggers = isBaseTypeTriggerable ? GetStaticTriggers(baseType) : null;

			void ITriggers.OnBeforeInsert(ITriggerable t, DbContext dbc) {
				baseTriggers?.OnBeforeInsert(t, dbc);
				Triggers<TTriggerable>.OnBeforeInsertStatic(t, dbc);
			}

			void ITriggers.OnBeforeUpdate(ITriggerable t, DbContext dbc) {
				baseTriggers?.OnBeforeUpdate(t, dbc);
				Triggers<TTriggerable>.OnBeforeUpdateStatic(t, dbc);
			}

			void ITriggers.OnBeforeDelete(ITriggerable t, DbContext dbc) {
				baseTriggers?.OnBeforeDelete(t, dbc);
				Triggers<TTriggerable>.OnBeforeDeleteStatic(t, dbc);
			}

			void ITriggers.OnInsertFailed(ITriggerable t, DbContext dbc, Exception ex) {
				baseTriggers?.OnInsertFailed(t, dbc, ex);
				Triggers<TTriggerable>.OnInsertFailedStatic(t, dbc, ex);
			}

			void ITriggers.OnUpdateFailed(ITriggerable t, DbContext dbc, Exception ex) {
				baseTriggers?.OnUpdateFailed(t, dbc, ex);
				Triggers<TTriggerable>.OnUpdateFailedStatic(t, dbc, ex);
			}

			void ITriggers.OnDeleteFailed(ITriggerable t, DbContext dbc, Exception ex) {
				baseTriggers?.OnDeleteFailed(t, dbc, ex);
				Triggers<TTriggerable>.OnDeleteFailedStatic(t, dbc, ex);
			}

			void ITriggers.OnAfterInsert(ITriggerable t, DbContext dbc) {
				baseTriggers?.OnAfterInsert(t, dbc);
				Triggers<TTriggerable>.OnAfterInsertStatic(t, dbc);
			}

			void ITriggers.OnAfterUpdate(ITriggerable t, DbContext dbc) {
				baseTriggers?.OnAfterUpdate(t, dbc);
				Triggers<TTriggerable>.OnAfterUpdateStatic(t, dbc);
			}

			void ITriggers.OnAfterDelete(ITriggerable t, DbContext dbc) {
				baseTriggers?.OnAfterDelete(t, dbc);
				Triggers<TTriggerable>.OnAfterDeleteStatic(t, dbc);
			}
		}
	}
}