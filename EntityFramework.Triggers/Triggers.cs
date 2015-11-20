using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace EntityFramework.Triggers {
	internal static class Triggers {
		private static readonly ConcurrentDictionary<Type, Func<ITriggers>> triggersConstructorCache = new ConcurrentDictionary<Type, Func<ITriggers>>();

		public static ITriggers Create(ITriggerable triggerable) {
			var triggersConstructor = triggersConstructorCache.GetOrAdd(triggerable.GetType(), TriggersConstructorFactory);
			return triggersConstructor();
		}

		private static Func<ITriggers> TriggersConstructorFactory(Type triggerableType) {
			return Expression.Lambda<Func<ITriggers>>(Expression.New(typeof(Triggers<>).MakeGenericType(triggerableType))).Compile();
		}
	}

	internal static class DbPropertyValuesWrapper<TTriggerable> where TTriggerable : class, ITriggerable
	{
		private static readonly Func<DbPropertyValues, TTriggerable> Factory = GetFactory();

		private static Func<DbPropertyValues, TTriggerable> GetFactory()
		{
			var generatedName = typeof (TTriggerable).Name + "__DbPropertyValuesWrapper";
            var assemblyName = new AssemblyName(generatedName + "Assembly");
			var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
			var moduleBuilder = assemblyBuilder.DefineDynamicModule(generatedName + "Module");
			var typeBuilder = moduleBuilder.DefineType(generatedName, TypeAttributes.Public | TypeAttributes.Class, typeof(TTriggerable));

			var properties = typeof (TTriggerable).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToArray();
			var virtualProperties = properties.Where(IsOverridable).ToArray();
			if (properties.Length != virtualProperties.Count())
				throw new Exception("Proxy entity object is only supported when all properties are virtual.");

			var fieldBuilder = typeBuilder.DefineField("dbPropertyValues", typeof (DbPropertyValues), FieldAttributes.Private | FieldAttributes.InitOnly);

			var constructorParameterTypes = new[] {typeof (DbPropertyValues)};
			var baseConstructor = typeof(TTriggerable).GetConstructor(Type.EmptyTypes);
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, constructorParameterTypes);
			var ilGenerator = constructorBuilder.GetILGenerator();
			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Call, baseConstructor);
			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Ldarg_1);
			ilGenerator.Emit(OpCodes.Stfld, fieldBuilder);
			ilGenerator.Emit(OpCodes.Ret);

			foreach (var property in virtualProperties) {
				GetProperty(typeBuilder, property, fieldBuilder);
			}

			var type = typeBuilder.CreateType();
			var constructor = type.GetConstructor(constructorParameterTypes);
			var parameter = Expression.Parameter(typeof (DbPropertyValues));
			return Expression.Lambda<Func<DbPropertyValues, TTriggerable>>(Expression.New(constructor, parameter), parameter).Compile();
		}

		private static void GetProperty(TypeBuilder typeBuilder, PropertyInfo property, FieldInfo dbPropertyValuesFieldInfo) {
			var getter = property.GetGetMethod();
			var getAndSetAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual;
            var getterBuilder = typeBuilder.DefineMethod(getter.Name, getAndSetAttributes, property.PropertyType, null);
			var ilGenerator = getterBuilder.GetILGenerator();
			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Ldfld, dbPropertyValuesFieldInfo);
			ilGenerator.Emit(OpCodes.Ldstr, property.Name);
			ilGenerator.Emit(OpCodes.Call, typeof(DbPropertyValues).GetMethod(nameof(DbPropertyValues.GetValue)).MakeGenericMethod(property.PropertyType));
			ilGenerator.Emit(OpCodes.Ret);

			var setter = property.GetSetMethod(nonPublic:true);
			var setterBuilder = typeBuilder.DefineMethod(setter.Name, getAndSetAttributes, null, new[] {property.PropertyType});
			ilGenerator = setterBuilder.GetILGenerator();
			ilGenerator.Emit(OpCodes.Ldstr, "Properties cannot be set on a proxy object of `OriginalValues`");
			ilGenerator.Emit(OpCodes.Newobj, typeof(NotImplementedException).GetConstructor(new [] {typeof(String)}));
			ilGenerator.Emit(OpCodes.Throw);
		}

		private static Boolean IsOverridable(PropertyInfo propertyInfo)
		{
			var getter = propertyInfo.GetGetMethod();
			return getter.IsVirtual || getter.IsAbstract;
		}

		public static TTriggerable Create(DbPropertyValues originalValues) { return Factory(originalValues); } 
	}

	public sealed class Triggers<TTriggerable> : ITriggers<TTriggerable>, ITriggers where TTriggerable : class, ITriggerable {
		internal Triggers() { }

		#region Entry implementations
		internal class Entry : IEntry<TTriggerable> {
			//[Obsolete("Please use the `Current` property. This property will be deprecated in the future.")]
			public TTriggerable Entity { get; internal set; }
			public DbContext Context { get; internal set; }
		}

		internal class AfterChangeEntry : Entry, IAfterChangeEntry<TTriggerable> {
			private readonly Object syncRoot = new Object();
			private TTriggerable original;
			public TTriggerable Original {
				get {
					lock (syncRoot)
						return original ?? (original = DbPropertyValuesWrapper<TTriggerable>.Create(Context.Entry(Entity).OriginalValues));
				}
			}
			
		}
		internal class FailedEntry : Entry, IFailedEntry<TTriggerable> {
			public Exception Exception { get; internal set; }
		}
		internal abstract class BeforeEntry : Entry, IBeforeEntry<TTriggerable> {
			public virtual void Cancel() => Context.Entry(Entity).State = EntityState.Unchanged;
		}
		internal class InsertingEntry : BeforeEntry { }
		internal class UpdatingEntry : BeforeEntry { }
		internal class DeletingEntry : BeforeEntry {
			public override void Cancel() => Context.Entry(Entity).State = EntityState.Modified;
		}
		#endregion
		#region Event helpers
		private static void Add<TIEntry>(List<Action<TIEntry>> eventHandlers, Action<TIEntry> value) {
			lock (eventHandlers)
				eventHandlers.Add(value);
		}

		private static void Remove<TIEntry>(List<Action<TIEntry>> eventHandlers, Action<TIEntry> value) {
			lock (eventHandlers)
				eventHandlers.Remove(value);
		}

		internal static void Raise<TIEntry>(List<Action<TIEntry>> eventHandlers, TIEntry entry) {
			List<Action<TIEntry>> eventHandlersCopy;
			lock (eventHandlers)
				eventHandlersCopy = new List<Action<TIEntry>>(eventHandlers);
			foreach (var eventHandler in eventHandlersCopy)
				eventHandler(entry);
		}
		#endregion
		#region Instance events
		private readonly List<Action<IBeforeEntry<TTriggerable>>> inserting = new List<Action<IBeforeEntry<TTriggerable>>>();
		private readonly List<Action<IBeforeEntry<TTriggerable>>> updating = new List<Action<IBeforeEntry<TTriggerable>>>();
		private readonly List<Action<IBeforeEntry<TTriggerable>>> deleting = new List<Action<IBeforeEntry<TTriggerable>>>();
		private readonly List<Action<IFailedEntry<TTriggerable>>> insertFailed = new List<Action<IFailedEntry<TTriggerable>>>();
		private readonly List<Action<IFailedEntry<TTriggerable>>> updateFailed = new List<Action<IFailedEntry<TTriggerable>>>();
		private readonly List<Action<IFailedEntry<TTriggerable>>> deleteFailed = new List<Action<IFailedEntry<TTriggerable>>>();
		private readonly List<Action<IEntry<TTriggerable>>> inserted = new List<Action<IEntry<TTriggerable>>>();
		private readonly List<Action<IAfterChangeEntry<TTriggerable>>> updated = new List<Action<IAfterChangeEntry<TTriggerable>>>();
		private readonly List<Action<IAfterChangeEntry<TTriggerable>>> deleted = new List<Action<IAfterChangeEntry<TTriggerable>>>();

		event Action<IBeforeEntry<TTriggerable>> ITriggers<TTriggerable>.Inserting { add { Add(inserting, value); } remove { Remove(inserting, value); } }
		event Action<IBeforeEntry<TTriggerable>> ITriggers<TTriggerable>.Updating { add { Add(updating, value); } remove { Remove(updating, value); } }
		event Action<IBeforeEntry<TTriggerable>> ITriggers<TTriggerable>.Deleting { add { Add(deleting, value); } remove { Remove(deleting, value); } }
		event Action<IFailedEntry<TTriggerable>> ITriggers<TTriggerable>.InsertFailed { add { Add(insertFailed, value); } remove { Remove(insertFailed, value); } }
		event Action<IFailedEntry<TTriggerable>> ITriggers<TTriggerable>.UpdateFailed { add { Add(updateFailed, value); } remove { Remove(updateFailed, value); } }
		event Action<IFailedEntry<TTriggerable>> ITriggers<TTriggerable>.DeleteFailed { add { Add(deleteFailed, value); } remove { Remove(deleteFailed, value); } }
		event Action<IEntry<TTriggerable>> ITriggers<TTriggerable>.Inserted { add { Add(inserted, value); } remove { Remove(inserted, value); } }
		event Action<IAfterChangeEntry<TTriggerable>> ITriggers<TTriggerable>.Updated { add { Add(updated, value); } remove { Remove(updated, value); } }
		event Action<IAfterChangeEntry<TTriggerable>> ITriggers<TTriggerable>.Deleted { add { Add(deleted, value); } remove { Remove(deleted, value); } }

		void ITriggers.OnBeforeInsert(ITriggerable t, DbContext dbc) => Raise(inserting, new InsertingEntry { Entity = (TTriggerable) t, Context = dbc });
		void ITriggers.OnBeforeUpdate(ITriggerable t, DbContext dbc) => Raise(updating, new UpdatingEntry { Entity = (TTriggerable) t, Context = dbc });
		void ITriggers.OnBeforeDelete(ITriggerable t, DbContext dbc) => Raise(deleting, new DeletingEntry { Entity = (TTriggerable) t, Context = dbc });
		void ITriggers.OnInsertFailed(ITriggerable t, DbContext dbc, Exception ex) => Raise(insertFailed, new FailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		void ITriggers.OnUpdateFailed(ITriggerable t, DbContext dbc, Exception ex) => Raise(updateFailed, new FailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		void ITriggers.OnDeleteFailed(ITriggerable t, DbContext dbc, Exception ex) => Raise(deleteFailed, new FailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		void ITriggers.OnAfterInsert(ITriggerable t, DbContext dbc) => Raise(inserted, new Entry { Entity = (TTriggerable) t, Context = dbc });
		void ITriggers.OnAfterUpdate(ITriggerable t, DbContext dbc) => Raise(updated, new AfterChangeEntry { Entity = (TTriggerable) t, Context = dbc });
		void ITriggers.OnAfterDelete(ITriggerable t, DbContext dbc) => Raise(deleted, new AfterChangeEntry { Entity = (TTriggerable) t, Context = dbc });
		#endregion
		#region Static events
		private static readonly List<Action<IBeforeEntry<TTriggerable>>> staticInserting = new List<Action<IBeforeEntry<TTriggerable>>>();
		private static readonly List<Action<IBeforeEntry<TTriggerable>>> staticUpdating = new List<Action<IBeforeEntry<TTriggerable>>>();
		private static readonly List<Action<IBeforeEntry<TTriggerable>>> staticDeleting = new List<Action<IBeforeEntry<TTriggerable>>>();
		private static readonly List<Action<IFailedEntry<TTriggerable>>> staticInsertFailed = new List<Action<IFailedEntry<TTriggerable>>>();
		private static readonly List<Action<IFailedEntry<TTriggerable>>> staticUpdateFailed = new List<Action<IFailedEntry<TTriggerable>>>();
		private static readonly List<Action<IFailedEntry<TTriggerable>>> staticDeleteFailed = new List<Action<IFailedEntry<TTriggerable>>>();
		private static readonly List<Action<IEntry<TTriggerable>>> staticInserted = new List<Action<IEntry<TTriggerable>>>();
		private static readonly List<Action<IAfterChangeEntry<TTriggerable>>> staticUpdated = new List<Action<IAfterChangeEntry<TTriggerable>>>();
		private static readonly List<Action<IAfterChangeEntry<TTriggerable>>> staticDeleted = new List<Action<IAfterChangeEntry<TTriggerable>>>();

		public static event Action<IBeforeEntry<TTriggerable>> Inserting { add { Add(staticInserting, value); } remove { Remove(staticInserting, value); } }
		public static event Action<IBeforeEntry<TTriggerable>> Updating { add { Add(staticUpdating, value); } remove { Remove(staticUpdating, value); } }
		public static event Action<IBeforeEntry<TTriggerable>> Deleting { add { Add(staticDeleting, value); } remove { Remove(staticDeleting, value); } }
		public static event Action<IFailedEntry<TTriggerable>> InsertFailed { add { Add(staticInsertFailed, value); } remove { Remove(staticInsertFailed, value); } }
		public static event Action<IFailedEntry<TTriggerable>> UpdateFailed { add { Add(staticUpdateFailed, value); } remove { Remove(staticUpdateFailed, value); } }
		public static event Action<IFailedEntry<TTriggerable>> DeleteFailed { add { Add(staticDeleteFailed, value); } remove { Remove(staticDeleteFailed, value); } }
		public static event Action<IEntry<TTriggerable>> Inserted { add { Add(staticInserted, value); } remove { Remove(staticInserted, value); } }
		public static event Action<IAfterChangeEntry<TTriggerable>> Updated { add { Add(staticUpdated, value); } remove { Remove(staticUpdated, value); } }
		public static event Action<IAfterChangeEntry<TTriggerable>> Deleted { add { Add(staticDeleted, value); } remove { Remove(staticDeleted, value); } }

		internal static void OnBeforeInsertStatic(ITriggerable t, DbContext dbc) => Raise(staticInserting, new InsertingEntry { Entity = (TTriggerable) t, Context = dbc });
		internal static void OnBeforeUpdateStatic(ITriggerable t, DbContext dbc) => Raise(staticUpdating, new UpdatingEntry { Entity = (TTriggerable) t, Context = dbc });
		internal static void OnBeforeDeleteStatic(ITriggerable t, DbContext dbc) => Raise(staticDeleting, new DeletingEntry { Entity = (TTriggerable) t, Context = dbc });
		internal static void OnInsertFailedStatic(ITriggerable t, DbContext dbc, Exception ex) => Raise(staticInsertFailed, new FailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		internal static void OnUpdateFailedStatic(ITriggerable t, DbContext dbc, Exception ex) => Raise(staticUpdateFailed, new FailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		internal static void OnDeleteFailedStatic(ITriggerable t, DbContext dbc, Exception ex) => Raise(staticDeleteFailed, new FailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		internal static void OnAfterInsertStatic(ITriggerable t, DbContext dbc) => Raise(staticInserted, new Entry { Entity = (TTriggerable) t, Context = dbc });
		internal static void OnAfterUpdateStatic(ITriggerable t, DbContext dbc) => Raise(staticUpdated, new AfterChangeEntry { Entity = (TTriggerable) t, Context = dbc });
		internal static void OnAfterDeleteStatic(ITriggerable t, DbContext dbc) => Raise(staticDeleted, new AfterChangeEntry { Entity = (TTriggerable) t, Context = dbc });
		#endregion
	}
}