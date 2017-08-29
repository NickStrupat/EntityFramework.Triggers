using System;
using System.Threading;
using System.Threading.Tasks;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers {
#else
using System.Data.Entity;
using System.Data.Common;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
namespace EntityFramework.Triggers {
#endif
	/// <summary>
	/// A <see cref="DbContext"/>-derived class with trigger functionality called automatically
	/// </summary>
	public abstract class DbContextWithTriggers : DbContext {
		public Boolean TriggersEnabled { get; set; } = true;
#if EF_CORE
		protected DbContextWithTriggers() : base() { }
		protected DbContextWithTriggers(DbContextOptions options) : base(options) { }

		public override Int32 SaveChanges() {
			return TriggersEnabled ? this.SaveChangesWithTriggers(base.SaveChanges) : base.SaveChanges();
		}
		
		public override Int32 SaveChanges(Boolean acceptAllChangesOnSuccess) {
			return TriggersEnabled ? this.SaveChangesWithTriggers(base.SaveChanges, acceptAllChangesOnSuccess) : base.SaveChanges(acceptAllChangesOnSuccess);
		}
		
		public override Task<Int32> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken)) {
			return TriggersEnabled ? this.SaveChangesWithTriggersAsync(base.SaveChangesAsync, cancellationToken) : base.SaveChangesAsync(cancellationToken);
		}
		
		public override Task<Int32> SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken)) {
			return TriggersEnabled ? this.SaveChangesWithTriggersAsync(base.SaveChangesAsync, acceptAllChangesOnSuccess, cancellationToken) : base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
		}
#else
		protected DbContextWithTriggers() : base() { }
		protected DbContextWithTriggers(DbCompiledModel model) : base(model) { }
		protected DbContextWithTriggers(String nameOrConnectionString) : base(nameOrConnectionString) { }
		protected DbContextWithTriggers(DbConnection existingConnection, Boolean contextOwnsConnection) : base(existingConnection, contextOwnsConnection) { }
		protected DbContextWithTriggers(ObjectContext objectContext, Boolean dbContextOwnsObjectContext) : base(objectContext, dbContextOwnsObjectContext) { }
		protected DbContextWithTriggers(String nameOrConnectionString, DbCompiledModel model) : base(nameOrConnectionString, model) { }
		protected DbContextWithTriggers(DbConnection existingConnection, DbCompiledModel model, Boolean contextOwnsConnection) : base(existingConnection, model, contextOwnsConnection) { }
		
		public override Int32 SaveChanges() {
			return TriggersEnabled ? this.SaveChangesWithTriggers(base.SaveChanges) : base.SaveChanges();
		}
		public override Task<Int32> SaveChangesAsync(CancellationToken cancellationToken) {
			return TriggersEnabled ? this.SaveChangesWithTriggersAsync(base.SaveChangesAsync, cancellationToken) : base.SaveChangesAsync(cancellationToken);
		}
#endif
	}
}
