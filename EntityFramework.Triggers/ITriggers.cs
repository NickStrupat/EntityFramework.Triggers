using System;
using System.Data.Entity;

namespace EntityFramework.Triggers {
	internal interface ITriggers {
		void OnBeforeInsert(ITriggerable triggerable, DbContext context);
		void OnBeforeUpdate(ITriggerable triggerable, DbContext context);
		void OnBeforeDelete(ITriggerable triggerable, DbContext context);
		void OnInsertFailed(ITriggerable triggerable, DbContext dbContext, Exception exception);
		void OnUpdateFailed(ITriggerable triggerable, DbContext dbContext, Exception exception);
		void OnDeleteFailed(ITriggerable triggerable, DbContext dbContext, Exception exception);
		void OnAfterInsert(ITriggerable triggerable, DbContext context);
		void OnAfterUpdate(ITriggerable triggerable, DbContext context);
		void OnAfterDelete(ITriggerable triggerable, DbContext context);
	}
}