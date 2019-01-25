using System;
using System.Threading.Tasks;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers {
#else
using System.Data.Entity;
namespace EntityFramework.Triggers {
#endif
	internal interface ITriggerEntityInvoker<TDbContext> where TDbContext : DbContext {
		void RaiseInserting   (IServiceProvider serviceProvider, Object entity, TDbContext dbc, ref Boolean cancel);
		void RaiseUpdating    (IServiceProvider serviceProvider, Object entity, TDbContext dbc, ref Boolean cancel);
		void RaiseDeleting    (IServiceProvider serviceProvider, Object entity, TDbContext dbc, ref Boolean cancel);
		void RaiseInsertFailed(IServiceProvider serviceProvider, Object entity, TDbContext dbc, Exception ex, ref Boolean swallow);
		void RaiseUpdateFailed(IServiceProvider serviceProvider, Object entity, TDbContext dbc, Exception ex, ref Boolean swallow);
		void RaiseDeleteFailed(IServiceProvider serviceProvider, Object entity, TDbContext dbc, Exception ex, ref Boolean swallow);
		void RaiseInserted    (IServiceProvider serviceProvider, Object entity, TDbContext dbc);
		void RaiseUpdated     (IServiceProvider serviceProvider, Object entity, TDbContext dbc);
		void RaiseDeleted     (IServiceProvider serviceProvider, Object entity, TDbContext dbc);

		Task<Boolean> RaiseInsertingAsync   (IServiceProvider serviceProvider, Object entity, TDbContext dbc, Boolean cancel);
		Task<Boolean> RaiseUpdatingAsync    (IServiceProvider serviceProvider, Object entity, TDbContext dbc, Boolean cancel);
		Task<Boolean> RaiseDeletingAsync    (IServiceProvider serviceProvider, Object entity, TDbContext dbc, Boolean cancel);
		Task<Boolean> RaiseInsertFailedAsync(IServiceProvider serviceProvider, Object entity, TDbContext dbc, Exception ex, Boolean swallow);
		Task<Boolean> RaiseUpdateFailedAsync(IServiceProvider serviceProvider, Object entity, TDbContext dbc, Exception ex, Boolean swallow);
		Task<Boolean> RaiseDeleteFailedAsync(IServiceProvider serviceProvider, Object entity, TDbContext dbc, Exception ex, Boolean swallow);
		Task          RaiseInsertedAsync    (IServiceProvider serviceProvider, Object entity, TDbContext dbc);
		Task          RaiseUpdatedAsync     (IServiceProvider serviceProvider, Object entity, TDbContext dbc);
		Task          RaiseDeletedAsync     (IServiceProvider serviceProvider, Object entity, TDbContext dbc);
	}
}