using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers.Tests {
#else
using System.Data.Entity;
using System.Data.Entity.Validation;
namespace EntityFramework.Triggers.Tests {
#endif
	public abstract class TestBase : IDisposable {
		protected abstract void Setup();
		protected abstract void Teardown();

		protected void DoATest(Action action) {
			if (!semaphoreSlim.Wait(10000))
				Assert.True(false, "Wait failed due to timeout");
			Setup();
			try {
				action();
			}
			finally {
				Teardown();
				semaphoreSlim.Release();
			}
		}

		protected async Task DoATestAsync(Func<Task> action) {
			if (!await semaphoreSlim.WaitAsync(10000).ConfigureAwait(false))
				Assert.True(false, "Wait failed due to timeout");
			Setup();
			try {
				await action().ConfigureAwait(false);
			}
			finally {
				Teardown();
				semaphoreSlim.Release();
			}
		}

		private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
		protected readonly Context Context = new Context();

		public virtual void Dispose() => Context.Dispose();
	}
}