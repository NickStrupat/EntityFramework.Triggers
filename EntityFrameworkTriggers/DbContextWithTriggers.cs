using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkTriggers {
    public abstract class DbContextWithTriggers : DbContext {
        protected DbContextWithTriggers() : base() {}
        protected DbContextWithTriggers(DbCompiledModel model) : base(model) {}
        protected DbContextWithTriggers(String nameOrConnectionString) : base(nameOrConnectionString) {}
        protected DbContextWithTriggers(ObjectContext objectContext, Boolean dbContextOwnsObjectContext) : base(objectContext, dbContextOwnsObjectContext) {}
        protected DbContextWithTriggers(String nameOrConnectionString, DbCompiledModel model) : base(nameOrConnectionString, model) {}
        protected DbContextWithTriggers(System.Data.Common.DbConnection existingConnection, Boolean contextOwnsConnection) : base(existingConnection, contextOwnsConnection) {}
        protected DbContextWithTriggers(System.Data.Common.DbConnection existingConnection, DbCompiledModel model, Boolean contextOwnsConnection) : base(existingConnection, model, contextOwnsConnection) {}

        private IEnumerable<Action> RaiseTheBeforeEvents()
        {
            var afterActions = new List<Action>();
            foreach (var entry in ChangeTracker.Entries<IEntityWithTriggers>()) {
                switch (entry.State) {
                    case EntityState.Added:
                        entry.Entity.OnBeforeInsert();
                        afterActions.Add(entry.Entity.OnAfterInsert);
                        break;
                    case EntityState.Deleted:
                        entry.Entity.OnBeforeDelete();
                        afterActions.Add(entry.Entity.OnAfterDelete);
                        break;
                    case EntityState.Modified:
                        entry.Entity.OnBeforeUpdate();
                        afterActions.Add(entry.Entity.OnAfterUpdate);
                        break;
                }
            }
            return afterActions;
        }
        private void RaiseTheAfterEvents(IEnumerable<Action> afterActions)
        {
            foreach (var afterAction in afterActions)
                afterAction();
        }

        public override Int32 SaveChanges() {
            var afterActions = RaiseTheBeforeEvents();
            var result = base.SaveChanges();
            RaiseTheAfterEvents(afterActions);
            return result;
        }

        public override async Task<Int32> SaveChangesAsync(CancellationToken cancellationToken)
        {
            var afterActions = RaiseTheBeforeEvents();
            var result = await base.SaveChangesAsync(cancellationToken);
            RaiseTheAfterEvents(afterActions);
            return result;
        }
    }
}