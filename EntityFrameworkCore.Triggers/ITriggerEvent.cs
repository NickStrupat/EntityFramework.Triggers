#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers
#else
using System.Data.Entity;
namespace EntityFramework.Triggers
#endif
{
    internal interface ITriggerEvent<TEntity, TDbContext>
    where TEntity : class
    where TDbContext : DbContext
    {}
}