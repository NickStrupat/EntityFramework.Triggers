#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers
#else
using System.Data.Entity;
namespace EntityFramework.Triggers
#endif
{
	public interface ITriggersBuilder
    {
        ITriggers<TEntity, TDbContext> Triggers<TEntity, TDbContext>()
        where TEntity : class
        where TDbContext : DbContext;

        ITriggers<TEntity> Triggers<TEntity>()
        where TEntity : class;

	    ITriggers Triggers();
    }
}