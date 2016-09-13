using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EntityFrameworkCore.Triggers {
	internal class TriggerInvoker<TDbContext> : ITriggerInvoker where TDbContext : DbContext {
		private static readonly Type BaseDbContextType = typeof(TDbContext).GetTypeInfo().BaseType;
		private static readonly Boolean IsADbContextType = typeof(DbContext).IsAssignableFrom(BaseDbContextType);
		private static readonly ITriggerInvoker BaseTriggerInvoker = IsADbContextType ? TriggerInvokers.Get(BaseDbContextType) : null;

		public List<Action<DbContext>> RaiseTheBeforeEvents(DbContext dbContext) {
			var theAfterEvents = BaseTriggerInvoker?.RaiseTheBeforeEvents(dbContext) ?? new List<Action<DbContext>>();

			var entries = dbContext.ChangeTracker.Entries();
			var context = (TDbContext) dbContext;
			foreach (var entry in entries) {
				var after = RaiseTheBeforeEvent(entry, context);
				if (after != null)
					theAfterEvents.Add((Action<DbContext>) after);
			}
			return theAfterEvents;
		}

		private Action<TDbContext> RaiseTheBeforeEvent(EntityEntry entry, TDbContext dbContext) {
			var triggers = TriggerEntityInvokers.Get(entry.Entity.GetType());
			switch (entry.State) {
				case EntityState.Added:
					triggers.RaiseBeforeInsert(entry.Entity, dbContext);
					if (entry.State == EntityState.Added)
						return context => triggers.RaiseAfterInsert(entry.Entity, context);
					break;
				case EntityState.Deleted:
					triggers.RaiseBeforeDelete(entry.Entity, dbContext);
					if (entry.State == EntityState.Deleted)
						return context => triggers.RaiseAfterDelete(entry.Entity, context);
					break;
				case EntityState.Modified:
					triggers.RaiseBeforeUpdate(entry.Entity, dbContext);
					if (entry.State == EntityState.Modified)
						return context => triggers.RaiseAfterUpdate(entry.Entity, context);
					break;
			}
			return null;
		}

		public void RaiseTheAfterEvents(DbContext dbContext, IEnumerable<Action<DbContext>> afterEvents) {
			foreach (var after in afterEvents)
				after(dbContext);
		}

		public Int32 BaseSaveChanges(DbContext dbContext, Boolean acceptAllChangesOnSuccess) {
			return ParentDbContext<TDbContext>.SaveChanges((TDbContext) dbContext, acceptAllChangesOnSuccess); // must call as TDbContext
		}

		public Task<Int32> BaseSaveChangesAsync(DbContext dbContext, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken) {
			return ParentDbContext<TDbContext>.SaveChangesAsync((TDbContext) dbContext, acceptAllChangesOnSuccess, cancellationToken); // must call as TDbContext
		}

		public void RaiseTheFailedEvents(DbContext dbContext, DbUpdateException dbUpdateException) {
			var context = (TDbContext) dbContext;
			foreach (var entry in dbUpdateException.Entries)
				RaiseTheFailedEvents(context, entry, dbUpdateException);
			foreach (var entry in dbUpdateException.Entries)
				RaiseTheFailedEvents(context, entry, dbUpdateException);
		}

		private static void RaiseTheFailedEvents(TDbContext dbContext, EntityEntry entry, Exception exception) {
			var triggers = TriggerEntityInvokers.Get(entry.Entity.GetType());
			switch (entry.State) {
				case EntityState.Added:
					triggers.RaiseInsertFailed(entry.Entity, dbContext, exception);
					break;
				case EntityState.Modified:
					triggers.RaiseUpdateFailed(entry.Entity, dbContext, exception);
					break;
				case EntityState.Deleted:
					triggers.RaiseDeleteFailed(entry.Entity, dbContext, exception);
					break;
			}
		}




		internal static class TriggerEntityInvokers {
			public static ITriggerEntityInvoker Get(Type entityType) => cache.GetOrAdd(entityType, ValueFactory);

			private static ITriggerEntityInvoker ValueFactory(Type type) => (ITriggerEntityInvoker)Activator.CreateInstance(typeof(TriggerEntityInvoker<>).MakeGenericType(type));

			private static readonly ConcurrentDictionary<Type, ITriggerEntityInvoker> cache = new ConcurrentDictionary<Type, ITriggerEntityInvoker>();
		}

		internal interface ITriggerEntityInvoker {
			void RaiseBeforeInsert(Object entity, TDbContext dbc);
			void RaiseBeforeUpdate(Object entity, TDbContext dbc);
			void RaiseBeforeDelete(Object entity, TDbContext dbc);
			void RaiseInsertFailed(Object entity, TDbContext dbc, Exception ex);
			void RaiseUpdateFailed(Object entity, TDbContext dbc, Exception ex);
			void RaiseDeleteFailed(Object entity, TDbContext dbc, Exception ex);
			void RaiseAfterInsert (Object entity, TDbContext dbc);
			void RaiseAfterUpdate (Object entity, TDbContext dbc);
			void RaiseAfterDelete (Object entity, TDbContext dbc);
		}

		internal class TriggerEntityInvoker<TEntity> : ITriggerEntityInvoker where TEntity : class {
			private static readonly Type BaseEntityType = typeof(TEntity).GetTypeInfo().BaseType;
			private static readonly Boolean HasBaseType = BaseEntityType == null;
			private static readonly ITriggerEntityInvoker BaseTriggerEntityInvoker = HasBaseType ? TriggerEntityInvokers.Get(BaseEntityType) : null;
			private static readonly ITriggerEntityInvoker[] DeclaredInterfaces = typeof(TEntity).GetDeclaredInterfaces().Select(TriggerEntityInvokers.Get).ToArray();

			void ITriggerEntityInvoker.RaiseBeforeInsert(Object entity, TDbContext dbc) {
				var e = (TEntity) entity;
				BaseTriggerEntityInvoker?.RaiseBeforeInsert(e, dbc);
				foreach (var declaredInterface in DeclaredInterfaces)
					declaredInterface.RaiseBeforeInsert(e, dbc);
				Triggers<TEntity, TDbContext>.RaiseBeforeInsert(e, dbc);
			}

			void ITriggerEntityInvoker.RaiseBeforeUpdate(Object entity, TDbContext dbc) {
				var e = (TEntity)entity;
				BaseTriggerEntityInvoker?.RaiseBeforeUpdate(e, dbc);
				foreach (var declaredInterface in DeclaredInterfaces)
					declaredInterface.RaiseBeforeUpdate(e, dbc);
				Triggers<TEntity, TDbContext>.RaiseBeforeUpdate(e, dbc);
			}

			void ITriggerEntityInvoker.RaiseBeforeDelete(Object entity, TDbContext dbc) {
				var e = (TEntity)entity;
				BaseTriggerEntityInvoker?.RaiseBeforeDelete(e, dbc);
				foreach (var declaredInterface in DeclaredInterfaces)
					declaredInterface.RaiseBeforeDelete(e, dbc);
				Triggers<TEntity, TDbContext>.RaiseBeforeDelete(e, dbc);
			}

			void ITriggerEntityInvoker.RaiseInsertFailed(Object entity, TDbContext dbc, Exception ex) {
				var e = (TEntity)entity;
				BaseTriggerEntityInvoker?.RaiseInsertFailed(e, dbc, ex);
				foreach (var declaredInterface in DeclaredInterfaces)
					declaredInterface.RaiseInsertFailed(e, dbc, ex);
				Triggers<TEntity, TDbContext>.RaiseInsertFailed((TEntity) entity, dbc, ex);
			}

			void ITriggerEntityInvoker.RaiseUpdateFailed(Object entity, TDbContext dbc, Exception ex) {
				var e = (TEntity)entity;
				BaseTriggerEntityInvoker?.RaiseUpdateFailed(e, dbc, ex);
				foreach (var declaredInterface in DeclaredInterfaces)
					declaredInterface.RaiseUpdateFailed(e, dbc, ex);
				Triggers<TEntity, TDbContext>.RaiseUpdateFailed((TEntity) entity, dbc, ex);
			}

			void ITriggerEntityInvoker.RaiseDeleteFailed(Object entity, TDbContext dbc, Exception ex) {
				var e = (TEntity)entity;
				BaseTriggerEntityInvoker?.RaiseDeleteFailed(e, dbc, ex);
				foreach (var declaredInterface in DeclaredInterfaces)
					declaredInterface.RaiseDeleteFailed(e, dbc, ex);
				Triggers<TEntity, TDbContext>.RaiseDeleteFailed((TEntity) entity, dbc, ex);
			}

			void ITriggerEntityInvoker.RaiseAfterInsert (Object entity, TDbContext dbc) {
				var e = (TEntity)entity;
				BaseTriggerEntityInvoker?.RaiseAfterInsert(e, dbc);
				foreach (var declaredInterface in DeclaredInterfaces)
					declaredInterface.RaiseAfterInsert(e, dbc);
				Triggers<TEntity, TDbContext>.RaiseAfterInsert(e, dbc);
			}

			void ITriggerEntityInvoker.RaiseAfterUpdate (Object entity, TDbContext dbc) {
				var e = (TEntity)entity;
				BaseTriggerEntityInvoker?.RaiseAfterUpdate(e, dbc);
				foreach (var declaredInterface in DeclaredInterfaces)
					declaredInterface.RaiseAfterUpdate(e, dbc);
				Triggers<TEntity, TDbContext>.RaiseAfterUpdate(e, dbc);
			}

			void ITriggerEntityInvoker.RaiseAfterDelete (Object entity, TDbContext dbc) {
				var e = (TEntity)entity;
				BaseTriggerEntityInvoker?.RaiseAfterDelete(e, dbc);
				foreach (var declaredInterface in DeclaredInterfaces)
					declaredInterface.RaiseAfterDelete(e, dbc);
				Triggers<TEntity, TDbContext>.RaiseAfterDelete(e, dbc);
			}
		}
	}
}