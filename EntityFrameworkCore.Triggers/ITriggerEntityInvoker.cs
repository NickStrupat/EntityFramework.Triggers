using System;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers {
#else
using System.Data.Entity;
namespace EntityFramework.Triggers {
#endif
	internal interface ITriggerEntityInvoker<TDbContext> where TDbContext : DbContext {
		void RaiseBeforeInsert(Object entity, TDbContext dbc);
		void RaiseBeforeUpdate(Object entity, TDbContext dbc);
		void RaiseBeforeDelete(Object entity, TDbContext dbc);
		Boolean RaiseInsertFailed(Object entity, TDbContext dbc, Exception ex, Boolean swallow);
		Boolean RaiseUpdateFailed(Object entity, TDbContext dbc, Exception ex, Boolean swallow);
		Boolean RaiseDeleteFailed(Object entity, TDbContext dbc, Exception ex, Boolean swallow);
		void RaiseAfterInsert (Object entity, TDbContext dbc);
		void RaiseAfterUpdate (Object entity, TDbContext dbc);
		void RaiseAfterDelete (Object entity, TDbContext dbc);

		void RaiseBeforeInsertInner(Object entry);
		void RaiseBeforeUpdateInner(Object entry);
		void RaiseBeforeDeleteInner(Object entry);
		Boolean RaiseInsertFailedInner(Object entry);
		Boolean RaiseUpdateFailedInner(Object entry);
		Boolean RaiseDeleteFailedInner(Object entry);
		void RaiseAfterInsertInner(Object entry);
		void RaiseAfterUpdateInner(Object entry);
		void RaiseAfterDeleteInner(Object entry);
	}
}