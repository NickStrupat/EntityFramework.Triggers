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
		List<Action<DbContext>> RaiseTheBeforeEventsOuter(DbContext dbContext);
		void RaiseTheBeforeEventsInner(DbContext dbContext, List<EntityEntry> entries, List<EntityEntry> triggeredEntries, List<Action<DbContext>> afterEvents);
		void RaiseTheAfterEvents(DbContext dbContext, IEnumerable<Action<DbContext>> afterEvents);
		void RaiseTheFailedEvents(DbContext dbContext, DbUpdateException dbUpdateException);
#if !EF_CORE
		void RaiseTheFailedEvents(DbContext dbContext, DbEntityValidationException dbEntityValidationException);
#endif
	}
}