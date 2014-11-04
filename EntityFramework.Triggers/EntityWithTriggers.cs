using System;
using System.Data.Entity;

namespace EntityFramework.Triggers {
	/// <summary>Base class for entities with trigger events</summary>
    /// <typeparam name="TEntity">Derived entity class (see: CRTP)</typeparam>
    /// <typeparam name="TDbContext">Derived context class (see: CRTP)</typeparam>
    public abstract class EntityWithTriggers<TEntity, TDbContext> : ITriggerable
		where TEntity : EntityWithTriggers<TEntity, TDbContext>, ITriggerable, new()
		where TDbContext : DbContext
	{
	    private readonly Triggers<TEntity, TDbContext> triggers;
	    protected EntityWithTriggers() {
		    triggers = ((TEntity) this).Triggers<TEntity, TDbContext>();
	    } 

		/// <summary>Raised just before this entity is added to the store</summary>
		public event Action<Triggers<TEntity, TDbContext>.BeforeEntry> Inserting {
			add { triggers.Inserting += value; }
			remove { triggers.Inserting -= value; }
		}
		/// <summary>Raised just before this entity is updated in the store</summary>
		public event Action<Triggers<TEntity, TDbContext>.BeforeEntry> Updating {
			add { triggers.Updating += value; }
			remove { triggers.Updating -= value; }
		}
		/// <summary>Raised just before this entity is deleted from the store</summary>
		public event Action<Triggers<TEntity, TDbContext>.BeforeEntry> Deleting {
			add { triggers.Deleting += value; }
			remove { triggers.Deleting -= value; }
		}
		/// <summary>Raised after Inserting event, but before Inserted event when an exception has occured while saving the changes to the store</summary>
		public event Action<Triggers<TEntity, TDbContext>.FailedEntry> InsertFailed {
			add { triggers.InsertFailed += value; }
			remove { triggers.InsertFailed -= value; }
		}
		/// <summary>Raised after Updating event, but before Updated event when an exception has occured while saving the changes to the store</summary>
		public event Action<Triggers<TEntity, TDbContext>.FailedEntry> UpdateFailed {
			add { triggers.UpdateFailed += value; }
			remove { triggers.UpdateFailed -= value; }
		}
		/// <summary>Raised after Deleting event, but before Deleted event when an exception has occured while saving the changes to the store</summary>
		public event Action<Triggers<TEntity, TDbContext>.FailedEntry> DeleteFailed {
			add { triggers.DeleteFailed += value; }
			remove { triggers.DeleteFailed -= value; }
		}
		/// <summary>Raised just after this entity is added to the store</summary>
		public event Action<Triggers<TEntity, TDbContext>.Entry> Inserted {
			add { triggers.Inserted += value; }
			remove { triggers.Inserted -= value; }
		}
		/// <summary>Raised just after this entity is updated in the store</summary>
		public event Action<Triggers<TEntity, TDbContext>.Entry> Updated {
			add { triggers.Updated += value; }
			remove { triggers.Updated -= value; }
		}
		/// <summary>Raised just after this entity is deleted from the store</summary>
		public event Action<Triggers<TEntity, TDbContext>.Entry> Deleted {
			add { triggers.Deleted += value; }
			remove { triggers.Deleted -= value; }
		}
    }
}
