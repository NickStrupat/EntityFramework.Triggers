using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif

namespace EntityFramework.Triggers {
	internal static class StaticTriggers {
		public static ITriggers GetStaticTriggers(Type triggerableType) => staticTriggersCache.GetOrAdd(triggerableType, StaticTriggersFactory);
		private static readonly ConcurrentDictionary<Type, ITriggers> staticTriggersCache = new ConcurrentDictionary<Type, ITriggers>();
		private static ITriggers StaticTriggersFactory(Type type) => (ITriggers)Activator.CreateInstance(typeof(StaticTriggersImplementation<>).MakeGenericType(type));

		private class StaticTriggersImplementation<TTriggerable> : ITriggers where TTriggerable : class, ITriggerable {
			private static Boolean IsITriggerable(Type type) => typeof (ITriggerable).IsAssignableFrom(type);
			private static IEnumerable<Type> GetITriggerableInterfaces(Type type) => type.GetInterfaces().Where(IsITriggerable);

			private static readonly Type type = typeof (TTriggerable);
			private static readonly Type baseType = type.BaseType;
			private static readonly ITriggers[] baseTriggersArray = IsITriggerable(baseType) ? new [] { GetStaticTriggers(baseType) } : new ITriggers[0];

			private static readonly Type[] baseTypeInterfaces = IsITriggerable(baseType) ? GetITriggerableInterfaces(baseType).ToArray() : new Type[0];
			private static readonly Type[] notInheritedInterfaces = GetITriggerableInterfaces(type).Except(baseTypeInterfaces).ToArray();
			private static readonly ITriggers[] notInheritedInterfacesTriggersArray = notInheritedInterfaces.Select(GetStaticTriggers).ToArray();
			private static readonly ITriggers[] triggersArray = baseTriggersArray.Concat(notInheritedInterfacesTriggersArray).ToArray();

			void ITriggers.OnBeforeInsert(ITriggerable t, DbContext dbc) {
				foreach (var triggers in triggersArray)
					triggers.OnBeforeInsert(t, dbc);
				Triggers<TTriggerable>.OnBeforeInsertStatic(t, dbc);
			}

			void ITriggers.OnBeforeUpdate(ITriggerable t, DbContext dbc) {
				foreach (var triggers in triggersArray)
					triggers.OnBeforeUpdate(t, dbc);
				Triggers<TTriggerable>.OnBeforeUpdateStatic(t, dbc);
			}

			void ITriggers.OnBeforeDelete(ITriggerable t, DbContext dbc) {
				foreach (var triggers in triggersArray)
					triggers.OnBeforeDelete(t, dbc);
				Triggers<TTriggerable>.OnBeforeDeleteStatic(t, dbc);
			}

			void ITriggers.OnInsertFailed(ITriggerable t, DbContext dbc, Exception ex) {
				foreach (var triggers in triggersArray)
					triggers.OnInsertFailed(t, dbc, ex);
				Triggers<TTriggerable>.OnInsertFailedStatic(t, dbc, ex);
			}

			void ITriggers.OnUpdateFailed(ITriggerable t, DbContext dbc, Exception ex) {
				foreach (var triggers in triggersArray)
					triggers.OnUpdateFailed(t, dbc, ex);
				Triggers<TTriggerable>.OnUpdateFailedStatic(t, dbc, ex);
			}

			void ITriggers.OnDeleteFailed(ITriggerable t, DbContext dbc, Exception ex) {
				foreach (var triggers in triggersArray)
					triggers.OnDeleteFailed(t, dbc, ex);
				Triggers<TTriggerable>.OnDeleteFailedStatic(t, dbc, ex);
			}

			void ITriggers.OnAfterInsert(ITriggerable t, DbContext dbc) {
				foreach (var triggers in triggersArray)
					triggers.OnAfterInsert(t, dbc);
				Triggers<TTriggerable>.OnAfterInsertStatic(t, dbc);
			}

			void ITriggers.OnAfterUpdate(ITriggerable t, DbContext dbc) {
				foreach (var triggers in triggersArray)
					triggers.OnAfterUpdate(t, dbc);
				Triggers<TTriggerable>.OnAfterUpdateStatic(t, dbc);
			}

			void ITriggers.OnAfterDelete(ITriggerable t, DbContext dbc) {
				foreach (var triggers in triggersArray)
					triggers.OnAfterDelete(t, dbc);
				Triggers<TTriggerable>.OnAfterDeleteStatic(t, dbc);
			}
		}
	}
}