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
		List<Action<DbContext>> RaiseTheBeforeEvents(DbContext dbContext, IServiceProvider serviceProvider);
		void RaiseTheBeforeEventInner(DbContext dbContext, IServiceProvider serviceProvider, EntityEntry entry, List<Action<DbContext>> afterEvents, ref Boolean cancel);

		void RaiseTheAfterEvents(DbContext dbContext, IServiceProvider serviceProvider, IEnumerable<Action<DbContext>> afterEvents);

		Boolean RaiseTheFailedEvents(DbContext dbContext, IServiceProvider serviceProvider, DbUpdateException dbUpdateException, ref Boolean swallow);
#if !EF_CORE
		Boolean RaiseTheFailedEvents(DbContext dbContext, IServiceProvider serviceProvider, DbEntityValidationException dbEntityValidationException, ref Boolean swallow);
#endif
		Boolean RaiseTheFailedEvents(DbContext dbContext, IServiceProvider serviceProvider, Exception exception, ref Boolean swallow);
	}
}