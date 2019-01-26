using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers
#else
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using EntityEntry = System.Data.Entity.Infrastructure.DbEntityEntry;
namespace EntityFramework.Triggers
#endif
{
	internal interface ITriggerInvokerAsync
	{
		Task<List<DelegateSynchronyUnion<DbContext>>> RaiseChangingEventsAsync(DbContext dbContext, IServiceProvider serviceProvider);

		Task RaiseChangedEventsAsync(DbContext dbContext, IServiceProvider serviceProvider, IEnumerable<DelegateSynchronyUnion<DbContext>> afterEvents);

		Task<Boolean> RaiseFailedEventsAsync(DbContext dbContext, IServiceProvider serviceProvider, DbUpdateException dbUpdateException);
#if !EF_CORE
		Task<Boolean> RaiseFailedEventsAsync(DbContext dbContext, IServiceProvider serviceProvider, DbEntityValidationException dbEntityValidationException);
#endif
		Task<Boolean> RaiseFailedEventsAsync(DbContext dbContext, IServiceProvider serviceProvider, Exception exception);
	}
}