using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers {
#else
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
namespace EntityFramework.Triggers {
#endif
	internal interface ITriggerInvoker {
		List<Action<DbContext>> RaiseTheBeforeEvents(DbContext dbContext);
		void RaiseTheAfterEvents(DbContext dbContext, IEnumerable<Action<DbContext>> afterEvents);

#if EF_CORE
		Int32 BaseSaveChanges(DbContext dbContext, Boolean acceptAllChangesOnSuccess);
		Task<Int32> BaseSaveChangesAsync(DbContext dbContext, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken);
#else
		Int32 BaseSaveChanges(DbContext dbContext);
#if !NET40
		Task<Int32> BaseSaveChangesAsync(DbContext dbContext, CancellationToken cancellationToken);
#endif
#endif

		void RaiseTheFailedEvents(DbContext dbContext, DbUpdateException dbUpdateException);
#if !EF_CORE
		void RaiseTheFailedEvents(DbContext dbContext, DbEntityValidationException dbEntityValidationException);
#endif
	}
}