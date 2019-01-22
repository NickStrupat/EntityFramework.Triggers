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

		void RaiseChangedEvents(DbContext dbContext, IServiceProvider serviceProvider, IEnumerable<List<Delegate>> afterEvents);

		Boolean RaiseFailedEvents(DbContext dbContext, IServiceProvider serviceProvider, DbUpdateException dbUpdateException, ref Boolean swallow);
#if !EF_CORE
		Boolean RaiseFailedEvents(DbContext dbContext, IServiceProvider serviceProvider, DbEntityValidationException dbEntityValidationException, ref Boolean swallow);
#endif
		Boolean RaiseFailedEvents(DbContext dbContext, IServiceProvider serviceProvider, Exception exception, ref Boolean swallow);
	}
}