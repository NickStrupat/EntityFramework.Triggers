using System;
using System.Collections.Generic;

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
		void RaiseTheFailedEvents(DbContext dbContext, DbUpdateException dbUpdateException);
#if !EF_CORE
		void RaiseTheFailedEvents(DbContext dbContext, DbEntityValidationException dbEntityValidationException);
#endif
	}
}