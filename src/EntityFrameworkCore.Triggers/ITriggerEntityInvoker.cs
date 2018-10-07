using System;

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
	}
}