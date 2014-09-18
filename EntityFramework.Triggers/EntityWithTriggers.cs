using System;

namespace EntityFramework.Triggers {
    /// <summary>Base class for entities which need events to fire before and after being added to, modified in, or removed from the store</summary>
    /// <typeparam name="TEntity">Derived entity class (see: CRTP)</typeparam>
    /// <typeparam name="TContext">Derived context class (see: CRTP)</typeparam>
    public abstract class EntityWithTriggers<TEntity, TContext> : Triggers<TEntity, TContext>, IEntityWithTriggers<TContext>
		where TEntity : EntityWithTriggers<TEntity, TContext>, new()
		where TContext : DbContextWithTriggers<TContext> {
	    protected EntityWithTriggers() {
		    Triggerable = (TEntity) this;
	    }
    }
}
