using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
#if NET40
using NUnit.Framework;
using Fact = NUnit.Framework.TestAttribute;
#else
using Xunit;
#endif

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers.Tests {
#else
using System.Data.Entity;
using System.Data.Entity.Validation;
namespace EntityFramework.Triggers.Tests {
#endif

	public class UnitTests {
		public static void Main(String[] args) { }
		// inserting
		// insertfailed
		// inserted
		// updating
		// updatefailed
		// updated
		// deleting
		// deletefailed
		// deleted

		// cancel inserting
		// cancel updating
		// cancel deleting

		// DbEntityValidationException
		// DbUpdateException

		// event order
		// inheritance hierarchy event order

		// original values on updating
		// firing 'before' triggers of an entity added by another's "before" trigger, all before actual SaveChanges is executed
		// TODO:
		// Cancelled property of "before" trigger
		// Swallow proprety of "failed" trigger
		// event loops
		// calling savechanges in an event handler
		// doubly-declared interfaces
	}

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

#if !NET40

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
#endif

		private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
		protected readonly Context Context = new Context();

		public virtual void Dispose() => Context.Dispose();
	}

	public abstract class ThingTestBase : TestBase {
		protected override void Setup() {
			Triggers<Thing>.Inserting += InsertingTrue;
			Triggers<Thing>.Inserting += InsertingCheckFlags;

			Triggers<Thing>.InsertFailed += InsertFailedTrue;
			Triggers<Thing>.InsertFailed += InsertFailedCheckFlags;

			Triggers<Thing>.Inserted += InsertedTrue;
			Triggers<Thing>.Inserted += InsertedCheckFlags;

			Triggers<Thing>.Updating += UpdatingTrue;
			Triggers<Thing>.Updating += UpdatingCheckFlags;

			Triggers<Thing>.UpdateFailed += UpdateFailedTrue;
			Triggers<Thing>.UpdateFailed += UpdateFailedCheckFlags;

			Triggers<Thing>.Updated += UpdatedTrue;
			Triggers<Thing>.Updated += UpdatedCheckFlags;

			Triggers<Thing>.Deleting += DeletingTrue;
			Triggers<Thing>.Deleting += DeletingCheckFlags;

			Triggers<Thing>.DeleteFailed += DeleteFailedTrue;
			Triggers<Thing>.DeleteFailed += DeleteFailedCheckFlags;

			Triggers<Thing>.Deleted += DeletedTrue;
			Triggers<Thing>.Deleted += DeletedCheckFlags;
		}

		protected override void Teardown() {
			Triggers<Thing>.Inserting    -= InsertingTrue;
			Triggers<Thing>.Inserting    -= InsertingCheckFlags;

			Triggers<Thing>.InsertFailed -= InsertFailedTrue;
			Triggers<Thing>.InsertFailed -= InsertFailedCheckFlags;

			Triggers<Thing>.Inserted     -= InsertedTrue;
			Triggers<Thing>.Inserted     -= InsertedCheckFlags;

			Triggers<Thing>.Updating     -= UpdatingTrue;
			Triggers<Thing>.Updating     -= UpdatingCheckFlags;

			Triggers<Thing>.UpdateFailed -= UpdateFailedTrue;
			Triggers<Thing>.UpdateFailed -= UpdateFailedCheckFlags;

			Triggers<Thing>.Updated      -= UpdatedTrue;
			Triggers<Thing>.Updated      -= UpdatedCheckFlags;

			Triggers<Thing>.Deleting     -= DeletingTrue;
			Triggers<Thing>.Deleting     -= DeletingCheckFlags;

			Triggers<Thing>.DeleteFailed -= DeleteFailedTrue;
			Triggers<Thing>.DeleteFailed -= DeleteFailedCheckFlags;

			Triggers<Thing>.Deleted      -= DeletedTrue;
			Triggers<Thing>.Deleted      -= DeletedCheckFlags;
		}

		private static void InsertingTrue         (IBeforeEntry<Thing>       e) => InsertingTrue         (e.Entity);
		private static void InsertingCheckFlags   (IBeforeEntry<Thing>       e) => InsertingCheckFlags   (e.Entity);
		private static void InsertFailedTrue      (IFailedEntry<Thing>       e) => InsertFailedTrue      (e.Entity);
		private static void InsertFailedCheckFlags(IFailedEntry<Thing>       e) => InsertFailedCheckFlags(e.Entity);
		private static void InsertedTrue          (IAfterEntry<Thing>        e) => InsertedTrue          (e.Entity);
		private static void InsertedCheckFlags    (IAfterEntry<Thing>        e) => InsertedCheckFlags    (e.Entity);
		private static void UpdatingTrue          (IBeforeChangeEntry<Thing> e) => UpdatingTrue          (e.Entity);
		private static void UpdatingCheckFlags    (IBeforeChangeEntry<Thing> e) => UpdatingCheckFlags    (e.Entity);
		private static void UpdateFailedTrue      (IChangeFailedEntry<Thing> e) => UpdateFailedTrue      (e.Entity);
		private static void UpdateFailedCheckFlags(IChangeFailedEntry<Thing> e) => UpdateFailedCheckFlags(e.Entity);
		private static void UpdatedTrue           (IAfterChangeEntry<Thing>  e) => UpdatedTrue           (e.Entity);
		private static void UpdatedCheckFlags     (IAfterChangeEntry<Thing>  e) => UpdatedCheckFlags     (e.Entity);
		private static void DeletingTrue          (IBeforeChangeEntry<Thing> e) => DeletingTrue          (e.Entity);
		private static void DeletingCheckFlags    (IBeforeChangeEntry<Thing> e) => DeletingCheckFlags    (e.Entity);
		private static void DeleteFailedTrue      (IChangeFailedEntry<Thing> e) => DeleteFailedTrue      (e.Entity);
		private static void DeleteFailedCheckFlags(IChangeFailedEntry<Thing> e) => DeleteFailedCheckFlags(e.Entity);
		private static void DeletedTrue           (IAfterChangeEntry<Thing>  e) => DeletedTrue           (e.Entity);
		private static void DeletedCheckFlags     (IAfterChangeEntry<Thing>  e) => DeletedCheckFlags     (e.Entity);

		public static void InsertingTrue         (Thing thing) => thing.Inserting = true;
		public static void InsertingCheckFlags   (Thing thing) => CheckFlags(thing, nameof(thing.Inserting));
		public static void InsertFailedTrue      (Thing thing) => thing.InsertFailed = true;
		public static void InsertFailedCheckFlags(Thing thing) => CheckFlags(thing, nameof(thing.Inserting), nameof(thing.InsertFailed));
		public static void InsertedTrue          (Thing thing) => thing.Inserted = true;
		public static void InsertedCheckFlags    (Thing thing) => CheckFlags(thing, nameof(thing.Inserting), nameof(thing.Inserted));
		public static void UpdatingTrue          (Thing thing) => thing.Updating = true;
		public static void UpdatingCheckFlags    (Thing thing) => CheckFlags(thing, nameof(thing.Updating));
		public static void UpdateFailedTrue      (Thing thing) => thing.UpdateFailed = true;
		public static void UpdateFailedCheckFlags(Thing thing) => CheckFlags(thing, nameof(thing.Updating), nameof(thing.UpdateFailed));
		public static void UpdatedTrue           (Thing thing) => thing.Updated = true;
		public static void UpdatedCheckFlags     (Thing thing) => CheckFlags(thing, nameof(thing.Updating), nameof(thing.Updated));
		public static void DeletingTrue          (Thing thing) => thing.Deleting = true;
		public static void DeletingCheckFlags    (Thing thing) => CheckFlags(thing, nameof(thing.Deleting));
		public static void DeleteFailedTrue      (Thing thing) => thing.DeleteFailed = true;
		public static void DeleteFailedCheckFlags(Thing thing) => CheckFlags(thing, nameof(thing.Deleting), nameof(thing.DeleteFailed));
		public static void DeletedTrue           (Thing thing) => thing.Deleted = true;
		public static void DeletedCheckFlags     (Thing thing) => CheckFlags(thing, nameof(thing.Deleting), nameof(thing.Deleted));

		private static readonly IEnumerable<PropertyInfo> FlagPropertyInfos = typeof(Thing).GetProperties().Where(x => x.PropertyType == typeof(Boolean));

		private static void CheckFlags(Thing thing, params String[] trueFlagNames) {
			foreach (var flagPropertyInfo in FlagPropertyInfos) {
				var flagSet = (Boolean) flagPropertyInfo.GetValue(thing, null);
				var failedAssertMessage = $"{flagPropertyInfo.Name}: {flagSet}";

				if (trueFlagNames.Contains(flagPropertyInfo.Name))
					Assert.True(flagSet, failedAssertMessage);
				else
					Assert.False(flagSet, failedAssertMessage);
			}
		}

		protected void ResetFlags(Thing thing) {
			foreach (var flag in FlagPropertyInfos)
				flag.SetValue(thing, false, null);
		}
	}

	public class AddRemoveEventHandler : TestBase {
		protected override void Setup() => Triggers<Thing, Context>.Inserting += TriggersOnInserting;
		protected override void Teardown() => Triggers<Thing, Context>.Inserting -= TriggersOnInserting;

		private Int32 triggerCount;
		private void TriggersOnInserting(IBeforeEntry<Thing> beforeEntry) => ++triggerCount;

		[Fact]
		public void Sync() => DoATest(() => {
			Context.Things.Add(new Thing { Value = "Foo" });
			Context.SaveChanges();
			Assert.True(1 == triggerCount);

			Teardown(); // Remove handler
			Context.Things.Add(new Thing { Value = "Foo" });
			Context.SaveChanges();
			Assert.True(1 == triggerCount);
		});

#if !NET40
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			Context.Things.Add(new Thing { Value = "Foo" });
			await Context.SaveChangesAsync().ConfigureAwait(false);
			Assert.True(1 == triggerCount);

			Teardown(); // Remove handler
			Context.Things.Add(new Thing { Value = "Foo" });
			await Context.SaveChangesAsync().ConfigureAwait(false);
			Assert.True(1 == triggerCount);
		});
#endif
	}

	public class Insert : ThingTestBase {
		[Fact]
		public void Sync() => DoATest(() => {
			var guid = Guid.NewGuid().ToString();
			var thing = new Thing { Value = guid };
			Context.Things.Add(thing);
			Context.SaveChanges();
			InsertedCheckFlags(thing);
			Assert.True(Context.Things.SingleOrDefault(x => x.Value == guid) != null);
		});

#if !NET40
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var guid = Guid.NewGuid().ToString();
			var thing = new Thing { Value = guid };
			Context.Things.Add(thing);
			await Context.SaveChangesAsync().ConfigureAwait(false);
			InsertedCheckFlags(thing);
			Assert.True(await Context.Things.SingleOrDefaultAsync(x => x.Value == guid).ConfigureAwait(false) != null);
		});
#endif
	}

	public class InsertFail : ThingTestBase {
		[Fact]
		public void Sync() => DoATest(() => {
			var thing = new Thing { Value = null };
			Context.Things.Add(thing);
			try {
				Context.SaveChanges();
			}
#if EF_CORE
			catch (DbUpdateException) {
#else
			catch (DbEntityValidationException) {
#endif
				InsertFailedCheckFlags(thing);
				return;
			}
			Assert.True(false, "Exception not caught");
		});

#if !NET40
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var thing = new Thing { Value = null };
			Context.Things.Add(thing);
			try {
				await Context.SaveChangesAsync().ConfigureAwait(false);
			}
#if EF_CORE
			catch (DbUpdateException) {
#else
			catch (DbEntityValidationException) {
#endif
				InsertFailedCheckFlags(thing);
				return;
			}
			Assert.True(false, "Exception not caught");
		});
#endif
	}

	public class InsertFailSwallow : TestBase {
		protected override void Setup() => Triggers<Thing>.InsertFailed += OnInsertFailed;
		protected override void Teardown() => Triggers<Thing>.InsertFailed -= OnInsertFailed;

		private static void OnInsertFailed(IFailedEntry<Thing, DbContext> e) {
#if EF_CORE
			Assert.True(e.Exception is DbUpdateException);
#else
			Assert.True(e.Exception is DbEntityValidationException);
#endif
			e.Swallow = true;
		}

		[Fact]
		public void Sync() => DoATest(() => {
			Context.Things.Add(new Thing { Value = null });
			Context.SaveChanges();
		});

#if !NET40
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			Context.Things.Add(new Thing { Value = null });
			await Context.SaveChangesAsync().ConfigureAwait(false);
		});
#endif
	}

	public class Update : ThingTestBase {
		[Fact]
		public void Sync() => DoATest(() => {
			var thing = new Thing { Value = "Foo" };
			Context.Things.Add(thing);
			Context.SaveChanges();
			thing.Value = "Bar";
			ResetFlags(thing);
			Context.SaveChanges();
			UpdatedCheckFlags(thing);
		});

#if !NET40
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var thing = new Thing { Value = "Foo" };
			Context.Things.Add(thing);
			await Context.SaveChangesAsync().ConfigureAwait(false);
			thing.Value = "Bar";
			ResetFlags(thing);
			await Context.SaveChangesAsync().ConfigureAwait(false);
			UpdatedCheckFlags(thing);
		});
#endif
	}

	public class UpdateFail : ThingTestBase {
		[Fact]
		public void Sync() => DoATest(() => {
			var thing = new Thing { Value = "Foo" };
			Context.Things.Add(thing);
			Context.SaveChanges();
			thing.Value = null;
			ResetFlags(thing);
			try {
				Context.SaveChanges();
			}
#if EF_CORE
			catch (DbUpdateException) {
#else
			catch (DbEntityValidationException) {
#endif
				UpdateFailedCheckFlags(thing);
				return;
			}
			Assert.True(false, "Exception not caught");
		});

#if !NET40
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var thing = new Thing { Value = "Foo" };
			Context.Things.Add(thing);
			await Context.SaveChangesAsync().ConfigureAwait(false);
			thing.Value = null;
			ResetFlags(thing);
			try {
				await Context.SaveChangesAsync().ConfigureAwait(false);
			}
#if EF_CORE
			catch (DbUpdateException) {
#else
			catch (DbEntityValidationException) {
#endif
				UpdateFailedCheckFlags(thing);
				return;
			}
			Assert.True(false, "Exception not caught");
		});
#endif
	}

	public class Delete : ThingTestBase {
		[Fact]
		public void Sync() => DoATest(() => {
			var thing = new Thing { Value = "Foo" };
			Context.Things.Add(thing);
			Context.SaveChanges();
			ResetFlags(thing);
			Context.Things.Remove(thing);
			Context.SaveChanges();
			DeletedCheckFlags(thing);
		});

#if !NET40
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var thing = new Thing { Value = "Foo" };
			Context.Things.Add(thing);
			await Context.SaveChangesAsync().ConfigureAwait(false);
			ResetFlags(thing);
			Context.Things.Remove(thing);
			await Context.SaveChangesAsync().ConfigureAwait(false);
			DeletedCheckFlags(thing);
		});
#endif
	}

	public class DeleteFail : ThingTestBase {
		protected override void Setup() {
			base.Setup();
			Triggers<Thing>.Deleting += OnDeleting;
		}

		protected override void Teardown() {
			Triggers<Thing>.Deleting -= OnDeleting;
			base.Teardown();
		}

		private static void OnDeleting(IBeforeChangeEntry<Thing, DbContext> e) {
			throw new Exception();
		}

		[Fact]
		public void Sync() => DoATest(() => {
			var thing = new Thing { Value = "Foo" };
			Context.Things.Add(thing);
			Context.SaveChanges();
			ResetFlags(thing);
			Context.Things.Remove(thing);
			try {
				Context.SaveChanges();
			}
			catch (Exception) {
				DeleteFailedCheckFlags(thing);
				return;
			}
			Assert.True(false, "Exception not caught");
		});

#if !NET40
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var thing = new Thing { Value = "Foo" };
			Context.Things.Add(thing);
			await Context.SaveChangesAsync().ConfigureAwait(false);
			ResetFlags(thing);
			Context.Things.Remove(thing);
			try {
				await Context.SaveChangesAsync().ConfigureAwait(false);
			}
			catch (Exception) {
				DeleteFailedCheckFlags(thing);
				return;
			}
			Assert.True(false, "Exception not caught");
		});
#endif
	}

	public class InsertingCancel : ThingTestBase {
		protected override void Setup() {
			base.Setup();
			Triggers<Thing>.Inserting += Cancel;
		}
		protected override void Teardown() {
			Triggers<Thing>.Inserting -= Cancel;
			base.Teardown();
		}

		protected virtual void Cancel(IBeforeEntry<Thing> e) => e.Cancel();

		[Fact]
		public void Sync() => DoATest(() => {
			var guid = Guid.NewGuid().ToString();
			var thing = new Thing { Value = guid };
			Context.Things.Add(thing);
			Context.SaveChanges();
			InsertingCheckFlags(thing);
			Assert.True(Context.Things.SingleOrDefault(x => x.Value == guid) == null);
		});

#if !NET40
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var guid = Guid.NewGuid().ToString();
			var thing = new Thing { Value = guid };
			Context.Things.Add(thing);
			await Context.SaveChangesAsync().ConfigureAwait(false);
			InsertingCheckFlags(thing);
			Assert.True(await Context.Things.SingleOrDefaultAsync(x => x.Value == guid).ConfigureAwait(false) == null);
		});
#endif
	}

	public class InsertingCancelledTrue : InsertingCancel {
		protected override void Cancel(IBeforeEntry<Thing> e) => e.Cancelled = true;

		[Fact]
		public new void Sync() => base.Sync();

#if !NET40
		[Fact]
		public new Task Async() => base.Async();
#endif
	}

	public class UpdatingCancel : ThingTestBase {
		protected override void Setup() {
			base.Setup();
			Triggers<Thing>.Updating += Cancel;
		}
		protected override void Teardown() {
			Triggers<Thing>.Updating -= Cancel;
			base.Teardown();
		}

		protected virtual void Cancel(IBeforeChangeEntry<Thing, DbContext> e) => e.Cancel();

		[Fact]
		public void Sync() => DoATest(() => {
			var guid = Guid.NewGuid().ToString();
			var thing = new Thing { Value = guid };
			Context.Things.Add(thing);
			Context.SaveChanges();
			ResetFlags(thing);
			var updatedGuid = Guid.NewGuid().ToString();
			thing.Value = updatedGuid;
			Context.SaveChanges();
			UpdatingCheckFlags(thing);
			Assert.True(Context.Things.SingleOrDefault(x => x.Value == guid) != null);
			Assert.True(Context.Things.SingleOrDefault(x => x.Value == updatedGuid) == null);
		});

#if !NET40
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var guid = Guid.NewGuid().ToString();
			var thing = new Thing { Value = guid };
			Context.Things.Add(thing);
			Context.SaveChanges();
			ResetFlags(thing);
			var updatedGuid = Guid.NewGuid().ToString();
			thing.Value = updatedGuid;
			await Context.SaveChangesAsync();
			UpdatingCheckFlags(thing);
			Assert.True(await Context.Things.SingleOrDefaultAsync(x => x.Value == guid).ConfigureAwait(false) != null);
			Assert.True(await Context.Things.SingleOrDefaultAsync(x => x.Value == updatedGuid).ConfigureAwait(false) == null);
		});
#endif
	}

	public class UpdatingCancelledTrue : UpdatingCancel {
		protected override void Cancel(IBeforeChangeEntry<Thing, DbContext> e) => e.Cancelled = true;

		[Fact]
		public new void Sync() => base.Sync();

#if !NET40
		[Fact]
		public new Task Async() => base.Async();
#endif
	}

	public class DeletingCancel : ThingTestBase {
		protected override void Setup() {
			base.Setup();
			Triggers<Thing>.Deleting += Cancel;
		}
		protected override void Teardown() {
			Triggers<Thing>.Deleting -= Cancel;
			base.Teardown();
		}

		protected virtual void Cancel(IBeforeChangeEntry<Thing, DbContext> e) => e.Cancel();

		[Fact]
		public void Sync() => DoATest(() => {
			var guid = Guid.NewGuid().ToString();
			var thing = new Thing { Value = guid };
			Context.Things.Add(thing);
			Context.SaveChanges();
			ResetFlags(thing);
			Context.Things.Remove(thing);
			Context.SaveChanges();
			DeletingCheckFlags(thing);
			Assert.True(Context.Things.SingleOrDefault(x => x.Value == guid) != null);
		});

#if !NET40
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var guid = Guid.NewGuid().ToString();
			var thing = new Thing { Value = guid };
			Context.Things.Add(thing);
			await Context.SaveChangesAsync();
			ResetFlags(thing);
			Context.Things.Remove(thing);
			await Context.SaveChangesAsync();
			DeletingCheckFlags(thing);
			Assert.True(await Context.Things.SingleOrDefaultAsync(x => x.Value == guid).ConfigureAwait(false) != null);
		});
#endif
	}

	public class DeletingCancelledTrue : DeletingCancel {
		protected override void Cancel(IBeforeChangeEntry<Thing, DbContext> e) => e.Cancelled = true;

		[Fact]
		public new void Sync() => base.Sync();

#if !NET40
		[Fact]
		public new Task Async() => base.Async();
#endif
	}

	public class EventFiringOrderRelativeToAttachment : TestBase {
		protected override void Setup() {
			Triggers<Thing>.Inserting += Add1;
			Triggers<Thing>.Inserting += Add2;
			Triggers<Thing>.Inserting += Add3;
		}

		protected override void Teardown() {
			Triggers<Thing>.Inserting -= Add1;
			Triggers<Thing>.Inserting -= Add2;
			Triggers<Thing>.Inserting -= Add3;
		}

		private static void Add1(IBeforeEntry<Thing, DbContext> e) => e.Entity.Numbers.Add(1);
		private static void Add2(IBeforeEntry<Thing, DbContext> e) => e.Entity.Numbers.Add(2);
		private static void Add3(IBeforeEntry<Thing, DbContext> e) => e.Entity.Numbers.Add(3);
		
		[Fact]
		public void Sync() => DoATest(() => {
			var thing = new Thing { Value = Guid.NewGuid().ToString() };
			Context.Things.Add(thing);
			Context.SaveChanges();
			Assert.True(thing.Numbers.SequenceEqual(new [] { 1, 2, 3 }));
		});

#if !NET40
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var thing = new Thing { Value = Guid.NewGuid().ToString() };
			Context.Things.Add(thing);
			await Context.SaveChangesAsync().ConfigureAwait(false);
			Assert.True(thing.Numbers.SequenceEqual(new[] { 1, 2, 3 }));
		});
#endif
	}

	public class EventFiringOrderRelativeToClassHierarchy : TestBase {
		protected override void Setup() {
			Triggers<RoyalGala>.Inserting += Add3;
			Triggers<Apple>.Inserting     += Add2;
			Triggers<Thing>.Inserting     += Add1;
		}

		protected override void Teardown() {
			Triggers<Thing>.Inserting     -= Add1;
			Triggers<Apple>.Inserting     -= Add2;
			Triggers<RoyalGala>.Inserting -= Add3;
		}

		private static void Add1(IBeforeEntry<Thing, DbContext> e) => e.Entity.Numbers.Add(1);
		private static void Add2(IBeforeEntry<Thing, DbContext> e) => e.Entity.Numbers.Add(2);
		private static void Add3(IBeforeEntry<Thing, DbContext> e) => e.Entity.Numbers.Add(3);

		[Fact]
		public void Sync() => DoATest(() => {
			var royalGala = new RoyalGala { Value = Guid.NewGuid().ToString() };
			Context.RoyalGalas.Add(royalGala);
			Context.SaveChanges();
			Assert.True(royalGala.Numbers.SequenceEqual(new[] { 1, 2, 3 }));
		});

#if !NET40
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var royalGala = new RoyalGala { Value = Guid.NewGuid().ToString() };
			Context.RoyalGalas.Add(royalGala);
			await Context.SaveChangesAsync().ConfigureAwait(false);
			Assert.True(royalGala.Numbers.SequenceEqual(new[] { 1, 2, 3 }));
		});
#endif
	}

	public class OriginalValuesOnUpdating : TestBase {
		protected override void Setup()    => Triggers<Thing>.Updating += TriggersOnUpdating;
		protected override void Teardown() => Triggers<Thing>.Updating -= TriggersOnUpdating;

		private void TriggersOnUpdating(IBeforeChangeEntry<Thing, DbContext> beforeChangeEntry) {
			Assert.True(beforeChangeEntry.Original.Value == guid);
			Assert.True(beforeChangeEntry.Entity.Value == guid2);
		}

		private String guid;
		private String guid2;

		[Fact]
		public void Sync() => DoATest(() => {
			guid = Guid.NewGuid().ToString();
			guid2 = Guid.NewGuid().ToString();
			var thing = new Thing { Value = guid };
			Context.Things.Add(thing);
			Context.SaveChanges();
			thing.Value = guid2;
			Context.SaveChanges();
		});

#if !NET40
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			guid = Guid.NewGuid().ToString();
			guid2 = Guid.NewGuid().ToString();
			var thing = new Thing { Value = guid };
			Context.Things.Add(thing);
			await Context.SaveChangesAsync().ConfigureAwait(false);
			thing.Value = guid2;
			await Context.SaveChangesAsync().ConfigureAwait(false);
		});
#endif
	}

	public class OriginalValuesOnDeleting : TestBase {
		protected override void Setup()    => Triggers<Thing>.Deleting += TriggersOnDeleting;
		protected override void Teardown() => Triggers<Thing>.Deleting -= TriggersOnDeleting;

		private void TriggersOnDeleting(IBeforeChangeEntry<Thing, DbContext> beforeChangeEntry) {
			Assert.True(beforeChangeEntry.Original.Value == guid);
			Assert.True(beforeChangeEntry.Entity.Value == guid2);
		}

		private String guid;
		private String guid2;

		[Fact]
		public void Sync() => DoATest(() => {
			guid = Guid.NewGuid().ToString();
			guid2 = Guid.NewGuid().ToString();
			var thing = new Thing { Value = guid };
			Context.Things.Add(thing);
			Context.SaveChanges();
			thing.Value = guid2;
			Context.Things.Remove(thing);
			Context.SaveChanges();
		});

#if !NET40
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			guid = Guid.NewGuid().ToString();
			guid2 = Guid.NewGuid().ToString();
			var thing = new Thing { Value = guid };
			Context.Things.Add(thing);
			await Context.SaveChangesAsync().ConfigureAwait(false);
			thing.Value = guid2;
			Context.Things.Remove(thing);
			await Context.SaveChangesAsync().ConfigureAwait(false);
		});
#endif
	}

//	public class MultiplyDeclaredInterfaces : TestBase {
//		protected override void Setup() {}
//		protected override void Teardown() { }

//		[Fact]
//		public void Sync() => DoATest(() => {
//		});

//#if !NET40
//		[Fact]
//		public Task Async() => DoATestAsync(async () => {
//		});
//#endif
//	}

//	public interface ICreature { }
//	public class Creature : ICreature {
//		[Key]
//		public virtual Int64 Id { get; protected set; }
//		public virtual String Name { get; set; }
//	}
//	public class Dog : Creature, ICreature { }
}