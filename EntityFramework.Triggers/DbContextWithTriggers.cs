using System;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFramework.Triggers {
	/// <summary>
	/// A <see cref="System.Data.Entity.DbContext"/> class with <see cref="Triggers{TTriggerable}"/> support
	/// </summary>
	public abstract class DbContextWithTriggers : DbContext {
		public override Int32 SaveChanges() {
			return this.SaveChangesWithTriggers(base.SaveChanges);
		}
#if !NET40
		public override Task<Int32> SaveChangesAsync(CancellationToken cancellationToken) {
			return this.SaveChangesWithTriggersAsync(base.SaveChangesAsync, cancellationToken);
		}
#endif
	}
}
