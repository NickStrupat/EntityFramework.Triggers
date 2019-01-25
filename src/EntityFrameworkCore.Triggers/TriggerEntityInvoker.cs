using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers
#else
using System.Data.Entity;
namespace EntityFramework.Triggers
#endif
{
	internal sealed class TriggerEntityInvoker<TDbContext, TEntity> : ITriggerEntityInvoker<TDbContext> where TDbContext : DbContext where TEntity : class
	{
		private static readonly Action<IInsertingEntry   <TEntity, TDbContext>, IServiceProvider> RaiseInsertingActions;
		private static readonly Action<IUpdatingEntry    <TEntity, TDbContext>, IServiceProvider> RaiseUpdatingActions;
		private static readonly Action<IDeletingEntry    <TEntity, TDbContext>, IServiceProvider> RaiseDeletingActions;
		private static readonly Action<IInsertFailedEntry<TEntity, TDbContext>, IServiceProvider> RaiseInsertFailedActions;
		private static readonly Action<IUpdateFailedEntry<TEntity, TDbContext>, IServiceProvider> RaiseUpdateFailedActions;
		private static readonly Action<IDeleteFailedEntry<TEntity, TDbContext>, IServiceProvider> RaiseDeleteFailedActions;
		private static readonly Action<IInsertedEntry    <TEntity, TDbContext>, IServiceProvider> RaiseInsertedActions;
		private static readonly Action<IUpdatedEntry     <TEntity, TDbContext>, IServiceProvider> RaiseUpdatedActions;
		private static readonly Action<IDeletedEntry     <TEntity, TDbContext>, IServiceProvider> RaiseDeletedActions;

		private static readonly Func<IInsertingEntry   <TEntity, TDbContext>, IServiceProvider, Task> RaiseInsertingActionsAsync;
		private static readonly Func<IUpdatingEntry    <TEntity, TDbContext>, IServiceProvider, Task> RaiseUpdatingActionsAsync;
		private static readonly Func<IDeletingEntry    <TEntity, TDbContext>, IServiceProvider, Task> RaiseDeletingActionsAsync;
		private static readonly Func<IInsertFailedEntry<TEntity, TDbContext>, IServiceProvider, Task> RaiseInsertFailedActionsAsync;
		private static readonly Func<IUpdateFailedEntry<TEntity, TDbContext>, IServiceProvider, Task> RaiseUpdateFailedActionsAsync;
		private static readonly Func<IDeleteFailedEntry<TEntity, TDbContext>, IServiceProvider, Task> RaiseDeleteFailedActionsAsync;
		private static readonly Func<IInsertedEntry    <TEntity, TDbContext>, IServiceProvider, Task> RaiseInsertedActionsAsync;
		private static readonly Func<IUpdatedEntry     <TEntity, TDbContext>, IServiceProvider, Task> RaiseUpdatedActionsAsync;
		private static readonly Func<IDeletedEntry     <TEntity, TDbContext>, IServiceProvider, Task> RaiseDeletedActionsAsync;

		static TriggerEntityInvoker()
		{
			(RaiseInsertingActions   , RaiseInsertingActionsAsync   ) = GetRaiseActions<IInsertingEntry   <TEntity, TDbContext>>(nameof(Triggers<Object, DbContext>.GlobalInserting   ), nameof(Triggers<Object, DbContext>.Inserting   ));
			(RaiseUpdatingActions    , RaiseUpdatingActionsAsync    ) = GetRaiseActions<IUpdatingEntry    <TEntity, TDbContext>>(nameof(Triggers<Object, DbContext>.GlobalUpdating    ), nameof(Triggers<Object, DbContext>.Updating    ));
			(RaiseDeletingActions    , RaiseDeletingActionsAsync    ) = GetRaiseActions<IDeletingEntry    <TEntity, TDbContext>>(nameof(Triggers<Object, DbContext>.GlobalDeleting    ), nameof(Triggers<Object, DbContext>.Deleting    ));
			(RaiseInsertFailedActions, RaiseInsertFailedActionsAsync) = GetRaiseActions<IInsertFailedEntry<TEntity, TDbContext>>(nameof(Triggers<Object, DbContext>.GlobalInsertFailed), nameof(Triggers<Object, DbContext>.InsertFailed));
			(RaiseUpdateFailedActions, RaiseUpdateFailedActionsAsync) = GetRaiseActions<IUpdateFailedEntry<TEntity, TDbContext>>(nameof(Triggers<Object, DbContext>.GlobalUpdateFailed), nameof(Triggers<Object, DbContext>.UpdateFailed));
			(RaiseDeleteFailedActions, RaiseDeleteFailedActionsAsync) = GetRaiseActions<IDeleteFailedEntry<TEntity, TDbContext>>(nameof(Triggers<Object, DbContext>.GlobalDeleteFailed), nameof(Triggers<Object, DbContext>.DeleteFailed));
			(RaiseInsertedActions    , RaiseInsertedActionsAsync    ) = GetRaiseActions<IInsertedEntry    <TEntity, TDbContext>>(nameof(Triggers<Object, DbContext>.GlobalInserted    ), nameof(Triggers<Object, DbContext>.Inserted    ));
			(RaiseUpdatedActions     , RaiseUpdatedActionsAsync     ) = GetRaiseActions<IUpdatedEntry     <TEntity, TDbContext>>(nameof(Triggers<Object, DbContext>.GlobalUpdated     ), nameof(Triggers<Object, DbContext>.Updated     ));
			(RaiseDeletedActions     , RaiseDeletedActionsAsync     ) = GetRaiseActions<IDeletedEntry     <TEntity, TDbContext>>(nameof(Triggers<Object, DbContext>.GlobalDeleted     ), nameof(Triggers<Object, DbContext>.Deleted     ));
		}

		private static (Action<TEntry, IServiceProvider> action, Func<TEntry, IServiceProvider, Task> func) GetRaiseActions<TEntry>(String globalTriggersEventName, String triggersEventName)
		where TEntry : IEntry<TEntity, TDbContext>
		{
			var pairs = GetTypePairs().ToArray();
			var raiseActions = new List<Action<TEntry, IServiceProvider>>(pairs.Length);
			var raiseFuncs = new List<Func<TEntry, IServiceProvider, Task>>(pairs.Length);
			foreach (var (dbContextType, entityType) in pairs)
			{
				var triggersType = typeof(Triggers<,>).MakeGenericType(entityType, dbContextType);
				var itriggersType = typeof(ITriggers<,>).MakeGenericType(entityType, dbContextType);

				var globalTriggerEventGetter =
					triggersType
						.GetProperty(globalTriggersEventName)
						.GetGetMethod()
						.CreateDelegate<Func<ITriggerEvent>>();

				var instanceTriggerEventGetter =
					typeof(ITriggers)
						.GetProperty(triggersEventName)
						.GetGetMethod()
						.CreateDelegate<Func<ITriggers, ITriggerEvent>>();


				void RaiseGlobalThenInstance(TEntry entry, IServiceProvider sp)
				{
					globalTriggerEventGetter().Raise(entry);
					if (sp?.GetService(itriggersType) is ITriggers triggers)
						instanceTriggerEventGetter(triggers).Raise(entry);
				}

				async Task RaiseGlobalThenInstanceAsync(TEntry entry, IServiceProvider sp)
				{
					await globalTriggerEventGetter().RaiseAsync(entry);
					if (sp?.GetService(itriggersType) is ITriggers triggers)
						await instanceTriggerEventGetter(triggers).RaiseAsync(entry);
				}

				raiseActions.Add(RaiseGlobalThenInstance);
				raiseFuncs.Add(RaiseGlobalThenInstanceAsync);
			}
			return (RaiseActions, RaiseFuncsAsync);

			void RaiseActions(TEntry entry, IServiceProvider sp)
			{
				foreach (var raiseAction in raiseActions)
					raiseAction(entry, sp);
			}

			async Task RaiseFuncsAsync(TEntry entry, IServiceProvider sp)
			{
				foreach (var raiseFunc in raiseFuncs)
					await raiseFunc(entry, sp);
			}

			IEnumerable<(Type dbContextType, Type entityType)> GetTypePairs()
			{
				var dbContextTypes = GetInheritanceChain<TDbContext>(includeInterfaces:false, typeof(DbContext));
				foreach (var entityType in GetInheritanceChain<TEntity>().Distinct())
				foreach (var dbContextType in dbContextTypes)
					yield return (dbContextType, entityType);
			}
		}

		private static List<Type> GetInheritanceChain<T>(Boolean includeInterfaces = true, Type terminator = null) where T : class
		{
			if (terminator == null)
				terminator = typeof(Object);
			var types = new List<Type>();
			for (var type = typeof(T);; type = type.BaseType)
			{
				types.Add(type);
				if (type == terminator)
					break;
				if (includeInterfaces)
					types.AddRange(type.GetDeclaredInterfaces().Reverse());
			}
			types.Reverse();
			return types;
		}

		public void RaiseInserting   (IServiceProvider sp, Object entity, TDbContext dbc, ref Boolean cancel)                { var entry = new InsertingEntry   <TEntity, TDbContext>((TEntity) entity, dbc, sp, cancel)     ; RaiseInsertingActions   (entry, sp); cancel = entry.Cancel; }
		public void RaiseUpdating    (IServiceProvider sp, Object entity, TDbContext dbc, ref Boolean cancel)                { var entry = new UpdatingEntry    <TEntity, TDbContext>((TEntity) entity, dbc, sp, cancel)     ; RaiseUpdatingActions    (entry, sp); cancel = entry.Cancel; }
		public void RaiseDeleting    (IServiceProvider sp, Object entity, TDbContext dbc, ref Boolean cancel)                { var entry = new DeletingEntry    <TEntity, TDbContext>((TEntity) entity, dbc, sp, cancel)     ; RaiseDeletingActions    (entry, sp); cancel = entry.Cancel; }
		public void RaiseInsertFailed(IServiceProvider sp, Object entity, TDbContext dbc, Exception ex, ref Boolean swallow) { var entry = new InsertFailedEntry<TEntity, TDbContext>((TEntity) entity, dbc, sp, ex, swallow); RaiseInsertFailedActions(entry, sp); swallow = entry.Swallow; }
		public void RaiseUpdateFailed(IServiceProvider sp, Object entity, TDbContext dbc, Exception ex, ref Boolean swallow) { var entry = new UpdateFailedEntry<TEntity, TDbContext>((TEntity) entity, dbc, sp, ex, swallow); RaiseUpdateFailedActions(entry, sp); swallow = entry.Swallow; }
		public void RaiseDeleteFailed(IServiceProvider sp, Object entity, TDbContext dbc, Exception ex, ref Boolean swallow) { var entry = new DeleteFailedEntry<TEntity, TDbContext>((TEntity) entity, dbc, sp, ex, swallow); RaiseDeleteFailedActions(entry, sp); swallow = entry.Swallow; }
		public void RaiseInserted    (IServiceProvider sp, Object entity, TDbContext dbc)                                    { var entry = new InsertedEntry    <TEntity, TDbContext>((TEntity) entity, dbc, sp)             ; RaiseInsertedActions    (entry, sp); }
		public void RaiseUpdated     (IServiceProvider sp, Object entity, TDbContext dbc)                                    { var entry = new UpdatedEntry     <TEntity, TDbContext>((TEntity) entity, dbc, sp)             ; RaiseUpdatedActions     (entry, sp); }
		public void RaiseDeleted     (IServiceProvider sp, Object entity, TDbContext dbc)                                    { var entry = new DeletedEntry     <TEntity, TDbContext>((TEntity) entity, dbc, sp)             ; RaiseDeletedActions     (entry, sp); }

		public async Task<Boolean> RaiseInsertingAsync   (IServiceProvider sp, Object entity, TDbContext dbc, Boolean cancel)                 { var entry = new InsertingEntry   <TEntity, TDbContext>((TEntity) entity, dbc, sp, cancel)     ; await RaiseInsertingActionsAsync   (entry, sp); return entry.Cancel; }
		public async Task<Boolean> RaiseUpdatingAsync    (IServiceProvider sp, Object entity, TDbContext dbc, Boolean cancel)                 { var entry = new UpdatingEntry    <TEntity, TDbContext>((TEntity) entity, dbc, sp, cancel)     ; await RaiseUpdatingActionsAsync    (entry, sp); return entry.Cancel; }
		public async Task<Boolean> RaiseDeletingAsync    (IServiceProvider sp, Object entity, TDbContext dbc, Boolean cancel)                 { var entry = new DeletingEntry    <TEntity, TDbContext>((TEntity) entity, dbc, sp, cancel)     ; await RaiseDeletingActionsAsync    (entry, sp); return entry.Cancel; }
		public async Task<Boolean> RaiseInsertFailedAsync(IServiceProvider sp, Object entity, TDbContext dbc, Exception ex, Boolean swallow)  { var entry = new InsertFailedEntry<TEntity, TDbContext>((TEntity) entity, dbc, sp, ex, swallow); await RaiseInsertFailedActionsAsync(entry, sp); return entry.Swallow; }
		public async Task<Boolean> RaiseUpdateFailedAsync(IServiceProvider sp, Object entity, TDbContext dbc, Exception ex, Boolean swallow)  { var entry = new UpdateFailedEntry<TEntity, TDbContext>((TEntity) entity, dbc, sp, ex, swallow); await RaiseUpdateFailedActionsAsync(entry, sp); return entry.Swallow; }
		public async Task<Boolean> RaiseDeleteFailedAsync(IServiceProvider sp, Object entity, TDbContext dbc, Exception ex, Boolean swallow)  { var entry = new DeleteFailedEntry<TEntity, TDbContext>((TEntity) entity, dbc, sp, ex, swallow); await RaiseDeleteFailedActionsAsync(entry, sp); return entry.Swallow; }
		public async Task          RaiseInsertedAsync    (IServiceProvider sp, Object entity, TDbContext dbc)                                 { var entry = new InsertedEntry    <TEntity, TDbContext>((TEntity) entity, dbc, sp)             ; await RaiseInsertedActionsAsync    (entry, sp); }
		public async Task          RaiseUpdatedAsync     (IServiceProvider sp, Object entity, TDbContext dbc)                                 { var entry = new UpdatedEntry     <TEntity, TDbContext>((TEntity) entity, dbc, sp)             ; await RaiseUpdatedActionsAsync     (entry, sp); }
		public async Task          RaiseDeletedAsync     (IServiceProvider sp, Object entity, TDbContext dbc)                                 { var entry = new DeletedEntry     <TEntity, TDbContext>((TEntity) entity, dbc, sp)             ; await RaiseDeletedActionsAsync     (entry, sp); }
	}
}