using System;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers {
#else
using System.Data.Entity;
namespace EntityFramework.Triggers {
#endif
	internal interface ITriggerEntityInvoker<TDbContext> where TDbContext : DbContext {
		void RaiseInserting(Object entity, TDbContext dbc);
		void RaiseUpdating(Object entity, TDbContext dbc);
		void RaiseDeleting(Object entity, TDbContext dbc);
		Boolean RaiseInsertFailed(Object entity, TDbContext dbc, Exception ex, Boolean swallow);
		Boolean RaiseUpdateFailed(Object entity, TDbContext dbc, Exception ex, Boolean swallow);
		Boolean RaiseDeleteFailed(Object entity, TDbContext dbc, Exception ex, Boolean swallow);
		void RaiseInserted (Object entity, TDbContext dbc);
		void RaiseUpdated (Object entity, TDbContext dbc);
		void RaiseDeleted (Object entity, TDbContext dbc);

		void RaiseInsertingInner(Object entry);
		void RaiseUpdatingInner(Object entry);
		void RaiseDeletingInner(Object entry);
		Boolean RaiseInsertFailedInner(Object entry);
		Boolean RaiseUpdateFailedInner(Object entry);
		Boolean RaiseDeleteFailedInner(Object entry);
		void RaiseInsertedInner(Object entry);
		void RaiseUpdatedInner(Object entry);
		void RaiseDeletedInner(Object entry);
	}
}