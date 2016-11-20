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

		public void RaiseInsertingInner(Object e) {
			var entry = (IInsertingEntry<TEntity, TDbContext>) e;
			BaseTriggerEntityInvoker?.RaiseInsertingInner(entry);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseInsertingInner(entry);
			Triggers<TEntity, TDbContext>.RaiseInserting(entry);
		}

		public void RaiseUpdatingInner(Object e) {
			var entry = (IUpdatingEntry<TEntity, TDbContext>) e;
			BaseTriggerEntityInvoker?.RaiseUpdatingInner(entry);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseUpdatingInner(entry);
			Triggers<TEntity, TDbContext>.RaiseUpdating(entry);
		}

		public void RaiseDeletingInner(Object e) {
			var entry = (IDeletingEntry<TEntity, TDbContext>) e;
			BaseTriggerEntityInvoker?.RaiseDeletingInner(entry);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseDeletingInner(entry);
			Triggers<TEntity, TDbContext>.RaiseDeleting(entry);
		}

		public void RaiseInsertFailedInner(Object e) {
			var entry = (IInsertFailedEntry<TEntity, TDbContext>) e;
			BaseTriggerEntityInvoker?.RaiseInsertFailedInner(entry);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseInsertFailedInner(entry);
			Triggers<TEntity, TDbContext>.RaiseInsertFailed(entry);
		}

		public void RaiseUpdateFailedInner(Object e) {
			var entry = (IUpdateFailedEntry<TEntity, TDbContext>) e;
			BaseTriggerEntityInvoker?.RaiseUpdateFailedInner(entry);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseUpdateFailedInner(entry);
			Triggers<TEntity, TDbContext>.RaiseUpdateFailed(entry);
		}

		public void RaiseDeleteFailedInner(Object e) {
			var entry = (IDeleteFailedEntry<TEntity, TDbContext>) e;
			BaseTriggerEntityInvoker?.RaiseDeleteFailedInner(entry);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseDeleteFailedInner(entry);
			Triggers<TEntity, TDbContext>.RaiseDeleteFailed(entry);
		}

		public void RaiseInsertedInner(Object e) {
			var entry = (IInsertedEntry<TEntity, TDbContext>) e;
			BaseTriggerEntityInvoker?.RaiseInsertedInner(entry);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseInsertedInner(entry);
			Triggers<TEntity, TDbContext>.RaiseInserted(entry);
		}

		public void RaiseUpdatedInner(Object e) {
			var entry = (IUpdatedEntry<TEntity, TDbContext>) e;
			BaseTriggerEntityInvoker?.RaiseUpdatedInner(entry);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseUpdatedInner(entry);
			Triggers<TEntity, TDbContext>.RaiseUpdated(entry);
		}

		public void RaiseDeletedInner(Object e) {
			var entry = (IDeletedEntry<TEntity, TDbContext>) e;
			BaseTriggerEntityInvoker?.RaiseDeletedInner(entry);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseDeletedInner(entry);
			Triggers<TEntity, TDbContext>.RaiseDeleted(entry);
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
			protected BeforeChangeEntry(TEntity entity, TDbContext context, Boolean cancel) : base(entity, context, cancel) {
				original = new Lazy<TEntity>(() => Context.GetOriginal(Entity));
			}
			private readonly Lazy<TEntity> original;
			public TEntity Original => original.Value;
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