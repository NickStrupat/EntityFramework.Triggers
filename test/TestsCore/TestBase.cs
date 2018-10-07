using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
using System.Data.Entity.Validation;
#endif

[assembly: CollectionBehavior(DisableTestParallelization = true)]

#if EF_CORE
namespace EntityFrameworkCore.Triggers.Tests {
#else
namespace EntityFramework.Triggers.Tests {
#endif
	public abstract class TestBase : IDisposable {
		protected abstract void Setup();
		protected abstract void Teardown();

		protected void DoATest(Action action) {
#if EF_CORE
			Context.Database.EnsureCreated();
#endif
			Setup();
			try {
				action();
			}
			finally {
				Teardown();
			}
		}

		protected async Task DoATestAsync(Func<Task> action) {
			Setup();
			try {
				await action().ConfigureAwait(false);
			}
			finally {
				Teardown();
			}
		}

		protected readonly Context Context = new Context(null);

		public virtual void Dispose() => Context.Dispose();
	}
}