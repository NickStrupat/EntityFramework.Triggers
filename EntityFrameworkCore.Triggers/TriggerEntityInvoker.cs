using System;
using System.Linq;
using System.Reflection;
using CoContra;
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

		public void    RaiseBeforeInsert(Object entity, TDbContext dbc)                                => RaiseBeforeInsertInner(new InsertingEntry   ((TEntity) entity, dbc));
		public void    RaiseBeforeUpdate(Object entity, TDbContext dbc)                                => RaiseBeforeUpdateInner(new UpdatingEntry    ((TEntity) entity, dbc));
		public void    RaiseBeforeDelete(Object entity, TDbContext dbc)                                => RaiseBeforeDeleteInner(new DeletingEntry    ((TEntity) entity, dbc));
		public Boolean RaiseInsertFailed(Object entity, TDbContext dbc, Exception ex, Boolean swallow) => RaiseInsertFailedInner(new InsertFailedEntry((TEntity) entity, dbc, ex, swallow));
		public Boolean RaiseUpdateFailed(Object entity, TDbContext dbc, Exception ex, Boolean swallow) => RaiseUpdateFailedInner(new UpdateFailedEntry((TEntity) entity, dbc, ex, swallow));
		public Boolean RaiseDeleteFailed(Object entity, TDbContext dbc, Exception ex, Boolean swallow) => RaiseDeleteFailedInner(new DeleteFailedEntry((TEntity) entity, dbc, ex, swallow));
		public void    RaiseAfterInsert (Object entity, TDbContext dbc)                                => RaiseAfterInsertInner (new InsertedEntry    ((TEntity) entity, dbc));
		public void    RaiseAfterUpdate (Object entity, TDbContext dbc)                                => RaiseAfterUpdateInner (new UpdatedEntry     ((TEntity) entity, dbc));
		public void    RaiseAfterDelete (Object entity, TDbContext dbc)                                => RaiseAfterDeleteInner (new DeletedEntry     ((TEntity) entity, dbc));

		public void RaiseBeforeInsertInner(Object e) {
			var entry = (InsertingEntry) e;
			BaseTriggerEntityInvoker?.RaiseBeforeInsertInner(entry);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseBeforeInsertInner(entry);
			Triggers<TEntity, TDbContext>.RaiseBeforeInsert(entry);
		}

		public void RaiseBeforeUpdateInner(Object e) {
			var entry = (UpdatingEntry) e;
			BaseTriggerEntityInvoker?.RaiseBeforeUpdateInner(entry);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseBeforeUpdateInner(entry);
			Triggers<TEntity, TDbContext>.RaiseBeforeUpdate(entry);
		}

		public void RaiseBeforeDeleteInner(Object e) {
			var entry = (DeletingEntry) e;
			BaseTriggerEntityInvoker?.RaiseBeforeDeleteInner(entry);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseBeforeDeleteInner(entry);
			Triggers<TEntity, TDbContext>.RaiseBeforeDelete(entry);
		}

		public Boolean RaiseInsertFailedInner(Object e) {
			var entry = (InsertFailedEntry) e;
			BaseTriggerEntityInvoker?.RaiseInsertFailedInner(entry);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseInsertFailedInner(entry);
			Triggers<TEntity, TDbContext>.RaiseInsertFailed(entry);
			return entry.Swallow;
		}

		public Boolean RaiseUpdateFailedInner(Object e) {
			var entry = (UpdateFailedEntry) e;
			BaseTriggerEntityInvoker?.RaiseUpdateFailedInner(entry);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseUpdateFailedInner(entry);
			Triggers<TEntity, TDbContext>.RaiseUpdateFailed(entry);
			return entry.Swallow;
		}

		public Boolean RaiseDeleteFailedInner(Object e) {
			var entry = (DeleteFailedEntry) e;
			BaseTriggerEntityInvoker?.RaiseDeleteFailedInner(entry);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseDeleteFailedInner(entry);
			Triggers<TEntity, TDbContext>.RaiseDeleteFailed(entry);
			return entry.Swallow;
		}

		public void RaiseAfterInsertInner(Object e) {
			var entry = (InsertedEntry) e;
			BaseTriggerEntityInvoker?.RaiseAfterInsertInner(entry);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseAfterInsertInner(entry);
			Triggers<TEntity, TDbContext>.RaiseAfterInsert(entry);
		}

		public void RaiseAfterUpdateInner(Object e) {
			var entry = (UpdatedEntry) e;
			BaseTriggerEntityInvoker?.RaiseAfterUpdateInner(entry);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseAfterUpdateInner(entry);
			Triggers<TEntity, TDbContext>.RaiseAfterUpdate(entry);
		}

		public void RaiseAfterDeleteInner(Object e) {
			var entry = (DeletedEntry) e;
			BaseTriggerEntityInvoker?.RaiseAfterDeleteInner(entry);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseAfterDeleteInner(entry);
			Triggers<TEntity, TDbContext>.RaiseAfterDelete(entry);
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

		private abstract class AfterEntry : Entry, IAfterEntry<TEntity, TDbContext> {
			protected AfterEntry(TEntity entity, TDbContext context) : base(entity, context) { }
		}

		private abstract class ChangeEntry : Entry, IChangeEntry<TEntity, TDbContext> {
			protected ChangeEntry(TEntity entity, TDbContext context) : base(entity, context) {
				original = new Lazy<TEntity>(() => Context.GetOriginal(Entity));
			}
			private readonly Lazy<TEntity> original;
			public TEntity Original => original.Value;
		}

		private abstract class AfterChangeEntry : ChangeEntry, IAfterChangeEntry<TEntity, TDbContext> {
			protected AfterChangeEntry(TEntity entity, TDbContext context) : base(entity, context) { }
		}

		private abstract class FailedEntry : Entry, IFailedEntry<TEntity, TDbContext> {
			protected FailedEntry(TEntity entity, TDbContext context, Exception exception, Boolean swallow) : base(entity, context) {
				Exception = exception;
				Swallow = swallow;
			}
			public Exception Exception { get; }
			public Boolean Swallow { get; set; }
		}

		private abstract class ChangeFailedEntry : ChangeEntry, IChangeFailedEntry<TEntity, TDbContext> {
			public Exception Exception { get; }
			public Boolean Swallow { get; set; }

			protected ChangeFailedEntry(TEntity entity, TDbContext context, Exception exception, Boolean swallow) : base(entity, context) {
				Exception = exception;
				Swallow = swallow;
			}
		}

		private class InsertingEntry : Entry, IInsertingEntry<TEntity, TDbContext> {
			public InsertingEntry(TEntity entity, TDbContext context) : base(entity, context) { }
			public void Cancel() => Cancelled = true;
			public Boolean Cancelled {
				get { return Context.Entry(Entity).State == EntityState.Detached; }
				set { Context.Entry(Entity).State = value ? EntityState.Detached : EntityState.Added; }
			}
		}

		private class UpdatingEntry : ChangeEntry, IUpdatingEntry<TEntity, TDbContext> {
			public UpdatingEntry(TEntity entity, TDbContext context) : base(entity, context) { }
			public void Cancel() => Cancelled = true;
			public Boolean Cancelled {
				get { return Context.Entry(Entity).State == EntityState.Unchanged; }
				set { Context.Entry(Entity).State = value ? EntityState.Unchanged : EntityState.Modified; }
			}
		}

		private class DeletingEntry : ChangeEntry, IDeletingEntry<TEntity, TDbContext> {
			public DeletingEntry(TEntity entity, TDbContext context) : base(entity, context) { }
			public void Cancel() => Cancelled = true;
			public Boolean Cancelled {
				get { return Context.Entry(Entity).State == EntityState.Modified; }
				set { Context.Entry(Entity).State = value ? EntityState.Modified : EntityState.Deleted; }
			}
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

		private class InsertedEntry : AfterEntry, IInsertedEntry<TEntity, TDbContext> {
			public InsertedEntry(TEntity entity, TDbContext context) : base(entity, context) {}
		}

		private class UpdatedEntry : AfterChangeEntry, IUpdatedEntry<TEntity, TDbContext> {
			public UpdatedEntry(TEntity entity, TDbContext context) : base(entity, context) {}
		}

		private class DeletedEntry : AfterChangeEntry, IDeletedEntry<TEntity, TDbContext> {
			public DeletedEntry(TEntity entity, TDbContext context) : base(entity, context) {}
		}
		#endregion
	}
}