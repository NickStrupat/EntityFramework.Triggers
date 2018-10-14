#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers.AspNetCore
#else
using System.Data.Entity;
namespace EntityFramework.Triggers.AspNetCore
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