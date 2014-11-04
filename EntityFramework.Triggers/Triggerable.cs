using System.Data.Entity;

namespace EntityFramework.Triggers {
	public abstract class Triggerable<TTriggerable, TDbContext> : ITriggerable
		where TTriggerable : Triggerable<TTriggerable, TDbContext>, ITriggerable
		where TDbContext : DbContext
	{
		public Triggers<TTriggerable, TDbContext> Triggers { get { return ((TTriggerable)this).Triggers<TTriggerable, TDbContext>(); } }
	}
}
