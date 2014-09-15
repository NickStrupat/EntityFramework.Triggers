using System;
using System.Data.Entity;

namespace EntityFrameworkTriggers {
	internal interface ITriggers<in TDbContext> where TDbContext : DbContext {
		void OnBeforeInsert(TDbContext context);
		void OnBeforeUpdate(TDbContext context);
		void OnBeforeDelete(TDbContext context);
		void OnInsertFailed(TDbContext dbContext, Exception exception);
		void OnUpdateFailed(TDbContext dbContext, Exception exception);
		void OnDeleteFailed(TDbContext dbContext, Exception exception);
		void OnAfterInsert(TDbContext context);
		void OnAfterUpdate(TDbContext context);
		void OnAfterDelete(TDbContext context);
	}
	internal interface ITriggers : ITriggers<DbContext> {}
}