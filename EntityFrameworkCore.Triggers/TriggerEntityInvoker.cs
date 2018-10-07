using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers
#else
using System.Data.Entity;
namespace EntityFramework.Triggers
#endif
{
	public sealed class TriggerEntityInvoker<TDbContext, TEntity> : ITriggerEntityInvoker<TDbContext> where TDbContext : DbContext where TEntity : class {
		private static readonly Action<IInsertingEntry   <TEntity, TDbContext>, IServiceProvider> RaiseInsertingActions    = GetRaiseActions<IInsertingEntry   <TEntity, TDbContext>>(nameof(Triggers<DbContext>.GlobalInserting   ), nameof(ITriggers<DbContext>.Inserting   ));
		private static readonly Action<IUpdatingEntry    <TEntity, TDbContext>, IServiceProvider> RaiseUpdatingActions     = GetRaiseActions<IUpdatingEntry    <TEntity, TDbContext>>(nameof(Triggers<DbContext>.GlobalUpdating    ), nameof(ITriggers<DbContext>.Updating    ));
		private static readonly Action<IDeletingEntry    <TEntity, TDbContext>, IServiceProvider> RaiseDeletingActions     = GetRaiseActions<IDeletingEntry    <TEntity, TDbContext>>(nameof(Triggers<DbContext>.GlobalDeleting    ), nameof(ITriggers<DbContext>.Deleting    ));
		private static readonly Action<IInsertFailedEntry<TEntity, TDbContext>, IServiceProvider> RaiseInsertFailedActions = GetRaiseActions<IInsertFailedEntry<TEntity, TDbContext>>(nameof(Triggers<DbContext>.GlobalInsertFailed), nameof(ITriggers<DbContext>.InsertFailed));
		private static readonly Action<IUpdateFailedEntry<TEntity, TDbContext>, IServiceProvider> RaiseUpdateFailedActions = GetRaiseActions<IUpdateFailedEntry<TEntity, TDbContext>>(nameof(Triggers<DbContext>.GlobalUpdateFailed), nameof(ITriggers<DbContext>.UpdateFailed));
		private static readonly Action<IDeleteFailedEntry<TEntity, TDbContext>, IServiceProvider> RaiseDeleteFailedActions = GetRaiseActions<IDeleteFailedEntry<TEntity, TDbContext>>(nameof(Triggers<DbContext>.GlobalDeleteFailed), nameof(ITriggers<DbContext>.DeleteFailed));
		private static readonly Action<IInsertedEntry    <TEntity, TDbContext>, IServiceProvider> RaiseInsertedActions     = GetRaiseActions<IInsertedEntry    <TEntity, TDbContext>>(nameof(Triggers<DbContext>.GlobalInserted    ), nameof(ITriggers<DbContext>.Inserted    ));
		private static readonly Action<IUpdatedEntry     <TEntity, TDbContext>, IServiceProvider> RaiseUpdatedActions      = GetRaiseActions<IUpdatedEntry     <TEntity, TDbContext>>(nameof(Triggers<DbContext>.GlobalUpdated     ), nameof(ITriggers<DbContext>.Updated     ));
		private static readonly Action<IDeletedEntry     <TEntity, TDbContext>, IServiceProvider> RaiseDeletedActions      = GetRaiseActions<IDeletedEntry     <TEntity, TDbContext>>(nameof(Triggers<DbContext>.GlobalDeleted     ), nameof(ITriggers<DbContext>.Deleted     ));

		private static Action<TEntry, IServiceProvider> GetRaiseActions<TEntry>(String globalTriggersEventName, String triggersEventName)
		where TEntry : IEntry<TEntity, TDbContext>
		{
			var pairs = GetTypePairs().ToArray();
			var raiseActions = new List<Action<TEntry, IServiceProvider>>(pairs.Length);
			foreach (var typePair in pairs)
			{
				var globalTriggerEventGetter = typeof(Triggers<,>).MakeGenericType(typePair.entityType, typePair.dbContextType).GetProperty(globalTriggersEventName).GetGetMethod().CreateDelegate<Func<ITriggerEvent>>();
				var instanceTriggerEventGetter = typeof(ITriggers).GetProperty(triggersEventName).GetGetMethod().CreateDelegate<Func<ITriggers, ITriggerEvent>>();
				var triggerType = typeof(ITriggers<,>).MakeGenericType(typePair.entityType, typePair.dbContextType);

				void RaiseGlobalThenInstance(TEntry entry, IServiceProvider sp)
				{
					globalTriggerEventGetter().Raise(entry, sp);
					if (sp?.GetService(triggerType) is ITriggers triggers)
						instanceTriggerEventGetter(triggers).Raise(entry, sp);
				}

				raiseActions.Add(RaiseGlobalThenInstance);
			}
			return RaiseActions;

			void RaiseActions(TEntry entry, IServiceProvider sp)
			{
				foreach (var raiseAction in raiseActions)
					raiseAction(entry, sp);
			}

			IEnumerable<(Type dbContextType, Type entityType)> GetTypePairs()
			{
				var dbContextTypes = GetInheritanceChain<TDbContext>(typeof(DbContext)).ToArray();
				foreach (var entityType in GetInheritanceChain<TEntity>())
				foreach (var dbContextType in dbContextTypes)
					yield return (dbContextType, entityType);
			}
		}

		private static IList<Type> GetInheritanceChain<T>(Type terminator = null) where T : class
		{
			if (terminator == null)
				terminator = typeof(Object);
			var types = new List<Type>();
			for (var type = typeof(T);; type = type.BaseType)
			{
				types.Add(type);
				if (type == terminator)
					break;
				types.AddRange(type.GetDeclaredInterfaces().Reverse());
			}
			types.Reverse();
			return types;
		}

		public void RaiseInserting   (IServiceProvider sp, Object entity, TDbContext dbc, ref Boolean cancel)                { var entry = new InsertingEntry   ((TEntity) entity, dbc, cancel)     ; RaiseInsertingActions   (entry, sp); cancel = entry.Cancel; }
		public void RaiseUpdating    (IServiceProvider sp, Object entity, TDbContext dbc, ref Boolean cancel)                { var entry = new UpdatingEntry    ((TEntity) entity, dbc, cancel)     ; RaiseUpdatingActions    (entry, sp); cancel = entry.Cancel; }
		public void RaiseDeleting    (IServiceProvider sp, Object entity, TDbContext dbc, ref Boolean cancel)                { var entry = new DeletingEntry    ((TEntity) entity, dbc, cancel)     ; RaiseDeletingActions    (entry, sp); cancel = entry.Cancel; }
		public void RaiseInsertFailed(IServiceProvider sp, Object entity, TDbContext dbc, Exception ex, ref Boolean swallow) { var entry = new InsertFailedEntry((TEntity) entity, dbc, ex, swallow); RaiseInsertFailedActions(entry, sp); swallow = entry.Swallow; }
		public void RaiseUpdateFailed(IServiceProvider sp, Object entity, TDbContext dbc, Exception ex, ref Boolean swallow) { var entry = new UpdateFailedEntry((TEntity) entity, dbc, ex, swallow); RaiseUpdateFailedActions(entry, sp); swallow = entry.Swallow; }
		public void RaiseDeleteFailed(IServiceProvider sp, Object entity, TDbContext dbc, Exception ex, ref Boolean swallow) { var entry = new DeleteFailedEntry((TEntity) entity, dbc, ex, swallow); RaiseDeleteFailedActions(entry, sp); swallow = entry.Swallow; }
		public void RaiseInserted    (IServiceProvider sp, Object entity, TDbContext dbc)                                    { var entry = new InsertedEntry    ((TEntity) entity, dbc)             ; RaiseInsertedActions    (entry, sp); }
		public void RaiseUpdated     (IServiceProvider sp, Object entity, TDbContext dbc)                                    { var entry = new UpdatedEntry     ((TEntity) entity, dbc)             ; RaiseUpdatedActions     (entry, sp); }
		public void RaiseDeleted     (IServiceProvider sp, Object entity, TDbContext dbc)                                    { var entry = new DeletedEntry     ((TEntity) entity, dbc)             ; RaiseDeletedActions     (entry, sp); }
		
		#region Entry implementations
		private abstract class Entry : IEntry<TEntity, TDbContext> {
			protected Entry(TEntity entity, TDbContext context) {
				Entity = entity;
				Context = context;
			}
			public TEntity Entity { get; }
			public TDbContext Context { get; }
			DbContext IEntry<TEntity>.Context => Context;
		}

		private abstract class BeforeEntry : Entry, IBeforeEntry<TEntity, TDbContext> {
			protected BeforeEntry(TEntity entity, TDbContext context, Boolean cancel) : base(entity, context) {
				Cancel = cancel;
			}
			public Boolean Cancel { get; set; }
		}

		private abstract class BeforeChangeEntry : BeforeEntry, IBeforeChangeEntry<TEntity, TDbContext> {
			protected BeforeChangeEntry(TEntity entity, TDbContext context, Boolean cancel) : base(entity, context, cancel) {}
			private TEntity original;
			public TEntity Original => original ?? (original = (TEntity)Context.Entry(Entity).OriginalValues.ToObject());
		}

		private abstract class FailedEntry : Entry, IFailedEntry<TEntity, TDbContext> {
			protected FailedEntry(TEntity entity, TDbContext context, Exception exception, Boolean swallow) : base(entity, context) {
				Exception = exception;
				Swallow = swallow;
			}
			public Exception Exception { get; }
			public Boolean Swallow { get; set; }
		}

		private abstract class ChangeFailedEntry : Entry, IChangeFailedEntry<TEntity, TDbContext> {
			public Exception Exception { get; }
			public Boolean Swallow { get; set; }

			protected ChangeFailedEntry(TEntity entity, TDbContext context, Exception exception, Boolean swallow) : base(entity, context) {
				Exception = exception;
				Swallow = swallow;
			}
		}
		

		private class InsertingEntry : BeforeEntry, IInsertingEntry<TEntity, TDbContext> {
			public InsertingEntry(TEntity entity, TDbContext context, Boolean cancel) : base(entity, context, cancel) { }
		}

		private class UpdatingEntry : BeforeChangeEntry, IUpdatingEntry<TEntity, TDbContext> {
			public UpdatingEntry(TEntity entity, TDbContext context, Boolean cancel) : base(entity, context, cancel) { }
		}

		private class DeletingEntry : BeforeChangeEntry, IDeletingEntry<TEntity, TDbContext> {
			public DeletingEntry(TEntity entity, TDbContext context, Boolean cancel) : base(entity, context, cancel) { }
		}

		private class InsertFailedEntry : FailedEntry, IInsertFailedEntry<TEntity, TDbContext> {
			public InsertFailedEntry(TEntity entity, TDbContext context, Exception exception, Boolean swallow) : base(entity, context, exception, swallow) {}
		}

		private class UpdateFailedEntry : ChangeFailedEntry, IUpdateFailedEntry<TEntity, TDbContext> {
			public UpdateFailedEntry(TEntity entity, TDbContext context, Exception exception, Boolean swallow) : base(entity, context, exception, swallow) {}
		}

		private class DeleteFailedEntry : ChangeFailedEntry, IDeleteFailedEntry<TEntity, TDbContext> {
			public DeleteFailedEntry(TEntity entity, TDbContext context, Exception exception, Boolean swallow) : base(entity, context, exception, swallow) {}
		}

		private class InsertedEntry : Entry, IInsertedEntry<TEntity, TDbContext> {
			public InsertedEntry(TEntity entity, TDbContext context) : base(entity, context) {}
		}

		private class UpdatedEntry : Entry, IUpdatedEntry<TEntity, TDbContext> {
			public UpdatedEntry(TEntity entity, TDbContext context) : base(entity, context) {}
		}

		private class DeletedEntry : Entry, IDeletedEntry<TEntity, TDbContext> {
			public DeletedEntry(TEntity entity, TDbContext context) : base(entity, context) {}
		}
		#endregion
	}
}