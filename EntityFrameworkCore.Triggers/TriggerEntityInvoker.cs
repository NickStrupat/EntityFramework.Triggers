using System;
using System.Linq;
using System.Reflection;
#if EF_CORE
using Microsoft.EntityFrameworkCore;
using EntityFrameworkCore.TypedOriginalValues;
namespace EntityFrameworkCore.Triggers {
#else
using System.Data.Entity;
using EntityFramework.TypedOriginalValues;
namespace EntityFramework.Triggers {
#endif

	internal class TriggerEntityInvoker<TDbContext, TEntity> : ITriggerEntityInvoker<TDbContext> where TDbContext : DbContext where TEntity : class {
		private static readonly Type BaseEntityType = typeof(TEntity).GetTypeInfo().BaseType;
		private static readonly ITriggerEntityInvoker<TDbContext> BaseTriggerEntityInvoker = BaseEntityType == null ? null : TriggerEntityInvokers<TDbContext>.Get(BaseEntityType);
		private static readonly ITriggerEntityInvoker<TDbContext>[] DeclaredInterfaces = typeof(TEntity).GetDeclaredInterfaces().Select(TriggerEntityInvokers<TDbContext>.Get).ToArray();

		public void RaiseInserting   (Object entity, TDbContext dbc, ref Boolean cancel)                { var entry = new InsertingEntry   ((TEntity) entity, dbc, cancel)     ; RaiseInsertingInner   (entry); cancel = entry.Cancel; }
		public void RaiseUpdating    (Object entity, TDbContext dbc, ref Boolean cancel)                { var entry = new UpdatingEntry    ((TEntity) entity, dbc, cancel)     ; RaiseUpdatingInner    (entry); cancel = entry.Cancel; }
		public void RaiseDeleting    (Object entity, TDbContext dbc, ref Boolean cancel)                { var entry = new DeletingEntry    ((TEntity) entity, dbc, cancel)     ; RaiseDeletingInner    (entry); cancel = entry.Cancel; }
		public void RaiseInsertFailed(Object entity, TDbContext dbc, Exception ex, ref Boolean swallow) { var entry = new InsertFailedEntry((TEntity) entity, dbc, ex, swallow); RaiseInsertFailedInner(entry); swallow = entry.Swallow; }
		public void RaiseUpdateFailed(Object entity, TDbContext dbc, Exception ex, ref Boolean swallow) { var entry = new UpdateFailedEntry((TEntity) entity, dbc, ex, swallow); RaiseUpdateFailedInner(entry); swallow = entry.Swallow; }
		public void RaiseDeleteFailed(Object entity, TDbContext dbc, Exception ex, ref Boolean swallow) { var entry = new DeleteFailedEntry((TEntity) entity, dbc, ex, swallow); RaiseDeleteFailedInner(entry); swallow = entry.Swallow; }
		public void RaiseInserted    (Object entity, TDbContext dbc)                                    { var entry = new InsertedEntry    ((TEntity) entity, dbc)             ; RaiseInsertedInner    (entry); }
		public void RaiseUpdated     (Object entity, TDbContext dbc)                                    { var entry = new UpdatedEntry     ((TEntity) entity, dbc)             ; RaiseUpdatedInner     (entry); }
		public void RaiseDeleted     (Object entity, TDbContext dbc)                                    { var entry = new DeletedEntry     ((TEntity) entity, dbc)             ; RaiseDeletedInner     (entry); }

		public void RaiseInsertingInner   (Object e) => RaiseInner<IInsertingEntry   <TEntity, TDbContext>>(e, i => i.RaiseInsertingInner   , Triggers<TEntity, TDbContext>.RaiseInserting   );
		public void RaiseUpdatingInner    (Object e) => RaiseInner<IUpdatingEntry    <TEntity, TDbContext>>(e, i => i.RaiseUpdatingInner    , Triggers<TEntity, TDbContext>.RaiseUpdating    );
		public void RaiseDeletingInner    (Object e) => RaiseInner<IDeletingEntry    <TEntity, TDbContext>>(e, i => i.RaiseDeletingInner    , Triggers<TEntity, TDbContext>.RaiseDeleting    );
		public void RaiseInsertFailedInner(Object e) => RaiseInner<IInsertFailedEntry<TEntity, TDbContext>>(e, i => i.RaiseInsertFailedInner, Triggers<TEntity, TDbContext>.RaiseInsertFailed);
		public void RaiseUpdateFailedInner(Object e) => RaiseInner<IUpdateFailedEntry<TEntity, TDbContext>>(e, i => i.RaiseUpdateFailedInner, Triggers<TEntity, TDbContext>.RaiseUpdateFailed);
		public void RaiseDeleteFailedInner(Object e) => RaiseInner<IDeleteFailedEntry<TEntity, TDbContext>>(e, i => i.RaiseDeleteFailedInner, Triggers<TEntity, TDbContext>.RaiseDeleteFailed);
		public void RaiseInsertedInner    (Object e) => RaiseInner<IInsertedEntry    <TEntity, TDbContext>>(e, i => i.RaiseInsertedInner    , Triggers<TEntity, TDbContext>.RaiseInserted    );
		public void RaiseUpdatedInner     (Object e) => RaiseInner<IUpdatedEntry     <TEntity, TDbContext>>(e, i => i.RaiseUpdatedInner     , Triggers<TEntity, TDbContext>.RaiseUpdated     );
		public void RaiseDeletedInner     (Object e) => RaiseInner<IDeletedEntry     <TEntity, TDbContext>>(e, i => i.RaiseDeletedInner     , Triggers<TEntity, TDbContext>.RaiseDeleted     );

		private void RaiseInner<TEntry>(Object e, Func<ITriggerEntityInvoker<TDbContext>, Action<TEntry>> getRaiseInner, Action<TEntry> raise) where TEntry : IEntry<TEntity, TDbContext>
		{
			var entry = (TEntry) e;
			if (BaseTriggerEntityInvoker != null)
				getRaiseInner(BaseTriggerEntityInvoker).Invoke(entry);
			foreach (var declaredInterface in DeclaredInterfaces)
				getRaiseInner(declaredInterface).Invoke(entry);
			raise(entry);
		}
		
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