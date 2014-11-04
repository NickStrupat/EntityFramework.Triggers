using System;
using System.Data.Entity;

namespace EntityFramework.Triggers {
	internal interface ITriggers<TDbContext> where TDbContext : DbContext {
		void OnBeforeInsert(ITriggerable triggerable, TDbContext context);
		void OnBeforeUpdate(ITriggerable triggerable, TDbContext context);
		void OnBeforeDelete(ITriggerable triggerable, TDbContext context);
		void OnInsertFailed(ITriggerable triggerable, TDbContext dbContext, Exception exception);
		void OnUpdateFailed(ITriggerable triggerable, TDbContext dbContext, Exception exception);
		void OnDeleteFailed(ITriggerable triggerable, TDbContext dbContext, Exception exception);
		void OnAfterInsert(ITriggerable triggerable, TDbContext context);
		void OnAfterUpdate(ITriggerable triggerable, TDbContext context);
		void OnAfterDelete(ITriggerable triggerable, TDbContext context);
	}
}