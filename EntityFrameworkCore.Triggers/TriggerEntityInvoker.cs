using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers {
#else
using System.Data.Entity;
namespace EntityFramework.Triggers {
#endif

	internal class TriggerEntityInvoker<TDbContext, TEntity> : ITriggerEntityInvoker<TDbContext> where TDbContext : DbContext where TEntity : class {
		private static readonly Type BaseEntityType = typeof(TEntity).GetTypeInfo().BaseType;
		private static readonly ITriggerEntityInvoker<TDbContext> BaseTriggerEntityInvoker = BaseEntityType == null ? null : TriggerEntityInvokers<TDbContext>.Get(BaseEntityType);
		private static readonly ITriggerEntityInvoker<TDbContext>[] DeclaredInterfaces = typeof(TEntity).GetDeclaredInterfaces().Select(TriggerEntityInvokers<TDbContext>.Get).ToArray();

		private static ITriggers<TEntity, TDbContext> GetTriggers(IServiceProvider serviceProvider) => serviceProvider.GetRequiredService<ITriggers<TEntity, TDbContext>>();

		public void RaiseInserting   (IServiceProvider sp, Object entity, TDbContext dbc, ref Boolean cancel)                { var entry = new InsertingEntry   ((TEntity) entity, dbc, cancel)     ; RaiseInsertingInner   (sp, entry); cancel = entry.Cancel; }
		public void RaiseUpdating    (IServiceProvider sp, Object entity, TDbContext dbc, ref Boolean cancel)                { var entry = new UpdatingEntry    ((TEntity) entity, dbc, cancel)     ; RaiseUpdatingInner    (sp, entry); cancel = entry.Cancel; }
		public void RaiseDeleting    (IServiceProvider sp, Object entity, TDbContext dbc, ref Boolean cancel)                { var entry = new DeletingEntry    ((TEntity) entity, dbc, cancel)     ; RaiseDeletingInner    (sp, entry); cancel = entry.Cancel; }
		public void RaiseInsertFailed(IServiceProvider sp, Object entity, TDbContext dbc, Exception ex, ref Boolean swallow) { var entry = new InsertFailedEntry((TEntity) entity, dbc, ex, swallow); RaiseInsertFailedInner(sp, entry); swallow = entry.Swallow; }
		public void RaiseUpdateFailed(IServiceProvider sp, Object entity, TDbContext dbc, Exception ex, ref Boolean swallow) { var entry = new UpdateFailedEntry((TEntity) entity, dbc, ex, swallow); RaiseUpdateFailedInner(sp, entry); swallow = entry.Swallow; }
		public void RaiseDeleteFailed(IServiceProvider sp, Object entity, TDbContext dbc, Exception ex, ref Boolean swallow) { var entry = new DeleteFailedEntry((TEntity) entity, dbc, ex, swallow); RaiseDeleteFailedInner(sp, entry); swallow = entry.Swallow; }
		public void RaiseInserted    (IServiceProvider sp, Object entity, TDbContext dbc)                                    { var entry = new InsertedEntry    ((TEntity) entity, dbc)             ; RaiseInsertedInner    (sp, entry); }
		public void RaiseUpdated     (IServiceProvider sp, Object entity, TDbContext dbc)                                    { var entry = new UpdatedEntry     ((TEntity) entity, dbc)             ; RaiseUpdatedInner     (sp, entry); }
		public void RaiseDeleted     (IServiceProvider sp, Object entity, TDbContext dbc)                                    { var entry = new DeletedEntry     ((TEntity) entity, dbc)             ; RaiseDeletedInner     (sp, entry); }

		public void RaiseInsertingInner   (IServiceProvider sp, Object e) => RaiseInner<IInsertingEntry   <TEntity, TDbContext>>(sp, e, i => i.RaiseInsertingInner   , t => t.Inserting   .Raise);
		public void RaiseUpdatingInner    (IServiceProvider sp, Object e) => RaiseInner<IUpdatingEntry    <TEntity, TDbContext>>(sp, e, i => i.RaiseUpdatingInner    , t => t.Updating    .Raise);
		public void RaiseDeletingInner    (IServiceProvider sp, Object e) => RaiseInner<IDeletingEntry    <TEntity, TDbContext>>(sp, e, i => i.RaiseDeletingInner    , t => t.Deleting    .Raise);
		public void RaiseInsertFailedInner(IServiceProvider sp, Object e) => RaiseInner<IInsertFailedEntry<TEntity, TDbContext>>(sp, e, i => i.RaiseInsertFailedInner, t => t.InsertFailed.Raise);
		public void RaiseUpdateFailedInner(IServiceProvider sp, Object e) => RaiseInner<IUpdateFailedEntry<TEntity, TDbContext>>(sp, e, i => i.RaiseUpdateFailedInner, t => t.UpdateFailed.Raise);
		public void RaiseDeleteFailedInner(IServiceProvider sp, Object e) => RaiseInner<IDeleteFailedEntry<TEntity, TDbContext>>(sp, e, i => i.RaiseDeleteFailedInner, t => t.DeleteFailed.Raise);
		public void RaiseInsertedInner    (IServiceProvider sp, Object e) => RaiseInner<IInsertedEntry    <TEntity, TDbContext>>(sp, e, i => i.RaiseInsertedInner    , t => t.Inserted    .Raise);
		public void RaiseUpdatedInner     (IServiceProvider sp, Object e) => RaiseInner<IUpdatedEntry     <TEntity, TDbContext>>(sp, e, i => i.RaiseUpdatedInner     , t => t.Updated     .Raise);
		public void RaiseDeletedInner     (IServiceProvider sp, Object e) => RaiseInner<IDeletedEntry     <TEntity, TDbContext>>(sp, e, i => i.RaiseDeletedInner     , t => t.Deleted     .Raise);

		private void RaiseInner<TEntry>(IServiceProvider sp, Object e, Func<ITriggerEntityInvoker<TDbContext>, Action<IServiceProvider, Object>> getRaiseInner, Func<ITriggers<TEntity, TDbContext>, Action<TEntry, IServiceProvider>> getRaise) where TEntry : IEntry<TEntity, TDbContext>
		{
			var entry = (TEntry) e;
			if (BaseTriggerEntityInvoker != null)
				getRaiseInner(BaseTriggerEntityInvoker).Invoke(sp, entry);
			foreach (var declaredInterface in DeclaredInterfaces)
				getRaiseInner(declaredInterface).Invoke(sp, entry);
			getRaise.Invoke(GetTriggers(sp)).Invoke(entry, sp);
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