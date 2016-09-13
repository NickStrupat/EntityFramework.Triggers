using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Triggers {
	internal interface ITriggerInvoker {
		List<Action<DbContext>> RaiseTheBeforeEvents(DbContext dbContext);
		void RaiseTheAfterEvents(DbContext dbContext, IEnumerable<Action<DbContext>> afterEvents);

		Int32 BaseSaveChanges(DbContext dbContext, Boolean acceptAllChangesOnSuccess);
		Task<Int32> BaseSaveChangesAsync(DbContext dbContext, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken);

		void RaiseTheFailedEvents(DbContext dbContext, DbUpdateException dbUpdateException);
	}
}