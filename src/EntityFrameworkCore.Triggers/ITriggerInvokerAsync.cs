using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Triggers;

internal interface ITriggerInvokerAsync
{
	Task<List<DelegateSynchronyUnion<DbContext>>> RaiseChangingEventsAsync(DbContext dbContext, IServiceProvider serviceProvider);

	Task RaiseChangedEventsAsync(DbContext dbContext, IServiceProvider serviceProvider, IEnumerable<DelegateSynchronyUnion<DbContext>> afterEvents);

	Task<Boolean> RaiseFailedEventsAsync(DbContext dbContext, IServiceProvider serviceProvider, DbUpdateException dbUpdateException);

	Task<Boolean> RaiseFailedEventsAsync(DbContext dbContext, IServiceProvider serviceProvider, Exception exception);
}