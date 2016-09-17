using System;
using System.Linq;
using System.Reflection;

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

		void ITriggerEntityInvoker<TDbContext>.RaiseBeforeInsert(Object entity, TDbContext dbc) {
			var e = (TEntity) entity;
			BaseTriggerEntityInvoker?.RaiseBeforeInsert(e, dbc);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseBeforeInsert(e, dbc);
			Triggers<TEntity, TDbContext>.RaiseBeforeInsert(e, dbc);
		}

		void ITriggerEntityInvoker<TDbContext>.RaiseBeforeUpdate(Object entity, TDbContext dbc) {
			var e = (TEntity) entity;
			BaseTriggerEntityInvoker?.RaiseBeforeUpdate(e, dbc);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseBeforeUpdate(e, dbc);
			Triggers<TEntity, TDbContext>.RaiseBeforeUpdate(e, dbc);
		}

		void ITriggerEntityInvoker<TDbContext>.RaiseBeforeDelete(Object entity, TDbContext dbc) {
			var e = (TEntity) entity;
			BaseTriggerEntityInvoker?.RaiseBeforeDelete(e, dbc);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseBeforeDelete(e, dbc);
			Triggers<TEntity, TDbContext>.RaiseBeforeDelete(e, dbc);
		}

		void ITriggerEntityInvoker<TDbContext>.RaiseInsertFailed(Object entity, TDbContext dbc, Exception ex) {
			var e = (TEntity) entity;
			BaseTriggerEntityInvoker?.RaiseInsertFailed(e, dbc, ex);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseInsertFailed(e, dbc, ex);
			Triggers<TEntity, TDbContext>.RaiseInsertFailed(e, dbc, ex);
		}

		void ITriggerEntityInvoker<TDbContext>.RaiseUpdateFailed(Object entity, TDbContext dbc, Exception ex) {
			var e = (TEntity) entity;
			BaseTriggerEntityInvoker?.RaiseUpdateFailed(e, dbc, ex);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseUpdateFailed(e, dbc, ex);
			Triggers<TEntity, TDbContext>.RaiseUpdateFailed(e, dbc, ex);
		}

		void ITriggerEntityInvoker<TDbContext>.RaiseDeleteFailed(Object entity, TDbContext dbc, Exception ex) {
			var e = (TEntity) entity;
			BaseTriggerEntityInvoker?.RaiseDeleteFailed(e, dbc, ex);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseDeleteFailed(e, dbc, ex);
			Triggers<TEntity, TDbContext>.RaiseDeleteFailed(e, dbc, ex);
		}

		void ITriggerEntityInvoker<TDbContext>.RaiseAfterInsert(Object entity, TDbContext dbc) {
			var e = (TEntity) entity;
			BaseTriggerEntityInvoker?.RaiseAfterInsert(e, dbc);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseAfterInsert(e, dbc);
			Triggers<TEntity, TDbContext>.RaiseAfterInsert(e, dbc);
		}

		void ITriggerEntityInvoker<TDbContext>.RaiseAfterUpdate(Object entity, TDbContext dbc) {
			var e = (TEntity) entity;
			BaseTriggerEntityInvoker?.RaiseAfterUpdate(e, dbc);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseAfterUpdate(e, dbc);
			Triggers<TEntity, TDbContext>.RaiseAfterUpdate(e, dbc);
		}

		void ITriggerEntityInvoker<TDbContext>.RaiseAfterDelete(Object entity, TDbContext dbc) {
			var e = (TEntity) entity;
			BaseTriggerEntityInvoker?.RaiseAfterDelete(e, dbc);
			foreach (var declaredInterface in DeclaredInterfaces)
				declaredInterface.RaiseAfterDelete(e, dbc);
			Triggers<TEntity, TDbContext>.RaiseAfterDelete(e, dbc);
		}
	}
}