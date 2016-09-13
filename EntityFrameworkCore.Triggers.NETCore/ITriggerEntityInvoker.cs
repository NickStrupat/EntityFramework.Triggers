using System;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Triggers {
	internal interface ITriggerEntityInvoker<TDbContext> where TDbContext : DbContext {
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
}