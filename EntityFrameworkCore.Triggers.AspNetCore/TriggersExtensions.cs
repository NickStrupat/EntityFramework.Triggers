using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EntityFrameworkCore.Triggers.AspNetCore
{
	public static class TriggersExtensions
	{
        public static IServiceCollection AddTriggers(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton(typeof(ITriggers<,>), typeof(Triggers<,>))
                                    .AddSingleton(typeof(ITriggers<>), typeof(Triggers<>));
        }

        public static IApplicationBuilder UseTriggers(this IApplicationBuilder app, Action<ITriggersBuilder> configureTriggers)
        {
            configureTriggers?.Invoke(new TriggersBuilder(app.ApplicationServices));
            return app;
        }

        private class TriggersBuilder : ITriggersBuilder
        {
            private readonly IServiceProvider serviceProvider;

            public TriggersBuilder(IServiceProvider serviceProvider) => this.serviceProvider = serviceProvider;

            public ITriggers<TEntity, TDbContext> Triggers<TEntity, TDbContext>()
            where TEntity : class
            where TDbContext : DbContext
            {
                return serviceProvider.GetService<ITriggers<TEntity, TDbContext>>();
            }

            public ITriggers<TEntity> Triggers<TEntity>()
            where TEntity : class
            {
                return serviceProvider.GetService<ITriggers<TEntity>>();
            }
        }
    }

    public interface ITriggersBuilder {
        ITriggers<TEntity, TDbContext> Triggers<TEntity, TDbContext>()
        where TEntity : class
        where TDbContext : DbContext;

        ITriggers<TEntity> Triggers<TEntity>()
        where TEntity : class;
    }
}
