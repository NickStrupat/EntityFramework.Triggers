using System;
using System.Collections.Generic;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
namespace EntityFrameworkCore.Triggers {
#else
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using EntityEntry = System.Data.Entity.Infrastructure.DbEntityEntry;
namespace EntityFramework.Triggers {
#endif
	internal interface ITriggerInvoker {
		List<Action<DbContext>> RaiseTheBeforeEvents(DbContext dbContext);
		void RaiseTheBeforeEventsInner(DbContext dbContext, List<EntityEntry> entries, List<EntityEntry> triggeredEntries, List<Action<DbContext>> afterEvents);
		void RaiseTheAfterEvents(DbContext dbContext, IEnumerable<Action<DbContext>> afterEvents);
		Boolean RaiseTheFailedEvents(DbContext dbContext, DbUpdateException dbUpdateException, Boolean swallow = false);
#if !EF_CORE
		Boolean RaiseTheFailedEvents(DbContext dbContext, DbEntityValidationException dbEntityValidationException, Boolean swallow = false);
#endif
		Boolean RaiseTheFailedEvents(DbContext dbContext, Exception exception, Boolean swallow = false);
	}
}