using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Triggers;

internal interface ITriggerInvoker {
	List<DelegateSynchronyUnion<DbContext>> RaiseChangingEvents(DbContext dbContext, IServiceProvider serviceProvider);

	void RaiseChangedEvents(DbContext dbContext, IServiceProvider serviceProvider, IEnumerable<DelegateSynchronyUnion<DbContext>> afterEvents);

	Boolean RaiseFailedEvents(DbContext dbContext, IServiceProvider serviceProvider, DbUpdateException dbUpdateException, ref Boolean swallow);

	Boolean RaiseFailedEvents(DbContext dbContext, IServiceProvider serviceProvider, Exception exception, ref Boolean swallow);
}