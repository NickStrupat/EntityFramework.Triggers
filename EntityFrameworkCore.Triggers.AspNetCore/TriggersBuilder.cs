using System;
using Microsoft.Extensions.DependencyInjection;
#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers.AspNetCore
#else
using System.Data.Entity;
namespace EntityFramework.Triggers.AspNetCore
#endif
{
    internal class TriggersBuilder : ITriggersBuilder
    {
        private readonly IServiceProvider serviceProvider;

        public TriggersBuilder(IServiceProvider serviceProvider) => this.serviceProvider = serviceProvider;

        public ITriggers<TEntity, TDbContext> Triggers<TEntity, TDbContext>()
        where TEntity : class
        where TDbContext : DbContext =>
            serviceProvider.GetRequiredService<ITriggers<TEntity, TDbContext>>();

        public ITriggers<TEntity> Triggers<TEntity>()
        where TEntity : class =>
            serviceProvider.GetRequiredService<ITriggers<TEntity>>();
    }
}