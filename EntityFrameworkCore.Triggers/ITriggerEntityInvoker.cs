using System;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers {
#else
using System.Data.Entity;
namespace EntityFramework.Triggers {
#endif
	internal interface ITriggerEntityInvoker<TDbContext> where TDbContext : DbContext {
		void RaiseInserting(Object entity, TDbContext dbc, ref Boolean cancel);
		void RaiseUpdating(Object entity, TDbContext dbc, ref Boolean cancel);
		void RaiseDeleting(Object entity, TDbContext dbc, ref Boolean cancel);
		void RaiseInsertFailed(Object entity, TDbContext dbc, Exception ex, ref Boolean swallow);
		void RaiseUpdateFailed(Object entity, TDbContext dbc, Exception ex, ref Boolean swallow);
		void RaiseDeleteFailed(Object entity, TDbContext dbc, Exception ex, ref Boolean swallow);
		void RaiseInserted (Object entity, TDbContext dbc);
		void RaiseUpdated (Object entity, TDbContext dbc);
		void RaiseDeleted (Object entity, TDbContext dbc);

		void RaiseInsertingInner(Object entry);
		void RaiseUpdatingInner(Object entry);
		void RaiseDeletingInner(Object entry);
		void RaiseInsertFailedInner(Object entry);
		void RaiseUpdateFailedInner(Object entry);
		void RaiseDeleteFailedInner(Object entry);
		void RaiseInsertedInner(Object entry);
		void RaiseUpdatedInner(Object entry);
		void RaiseDeletedInner(Object entry);
	}
}