﻿using System;
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
using System.Data.Entity.Infrastructure;
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

		// TODO:
		// original values on updating
		// doubly-declared interfaces
		// event loops
		// calling savechanges in an event handler
	}

	public abstract class TestBase : IDisposable {
		protected abstract void Setup();
		protected abstract void Teardown();

		private void SetupInternal() {
			semaphoreSlim.Wait();
			Setup();
		}

		private void TearDownInternal() {
			Teardown();
			semaphoreSlim.Release();
		}

		protected void DoATest(Action action) {
			try {
				SetupInternal();
				action();
			}
			finally {
				TearDownInternal();
			}
		}

#if !NET40
		private async Task SetupInternalAsync() {
			await semaphoreSlim.WaitAsync();
			Setup();
		}

		protected async Task DoATestAsync(Func<Task> action) {
			try {
				await SetupInternalAsync();
				await action();
			}
			finally {
				TearDownInternal();
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

		private static void InsertingTrue         (IBeforeEntry<Thing, DbContext>       e) => e.Entity.Inserting = true;
		private static void InsertingCheckFlags   (IBeforeEntry<Thing, DbContext>       e) => CheckFlags(e.Entity, nameof(e.Entity.Inserting));
		private static void InsertFailedTrue      (IFailedEntry<Thing, DbContext>       e) => e.Entity.InsertFailed = true;
		private static void InsertFailedCheckFlags(IFailedEntry<Thing, DbContext>       e) => CheckFlags(e.Entity, nameof(e.Entity.Inserting), nameof(e.Entity.InsertFailed));
		private static void InsertedTrue          (IAfterEntry<Thing, DbContext>        e) => e.Entity.Inserted = true;
		private static void InsertedCheckFlags    (IAfterEntry<Thing, DbContext>        e) => CheckFlags(e.Entity, nameof(e.Entity.Inserting), nameof(e.Entity.Inserted));
		private static void UpdatingTrue          (IBeforeChangeEntry<Thing, DbContext> e) => e.Entity.Updating = true;
		private static void UpdatingCheckFlags    (IBeforeChangeEntry<Thing, DbContext> e) => CheckFlags(e.Entity, nameof(e.Entity.Updating));
		private static void UpdateFailedTrue      (IChangeFailedEntry<Thing, DbContext> e) => e.Entity.UpdateFailed = true;
		private static void UpdateFailedCheckFlags(IChangeFailedEntry<Thing, DbContext> e) => CheckFlags(e.Entity, nameof(e.Entity.Updating), nameof(e.Entity.UpdateFailed));
		private static void UpdatedTrue           (IAfterChangeEntry<Thing, DbContext>  e) => e.Entity.Updated = true;
		private static void UpdatedCheckFlags     (IAfterChangeEntry<Thing, DbContext>  e) => CheckFlags(e.Entity, nameof(e.Entity.Updating), nameof(e.Entity.Updated));
		private static void DeletingTrue          (IBeforeChangeEntry<Thing, DbContext> e) => e.Entity.Deleting = true;
		private static void DeletingCheckFlags    (IBeforeChangeEntry<Thing, DbContext> e) => CheckFlags(e.Entity, nameof(e.Entity.Deleting));
		private static void DeleteFailedTrue      (IChangeFailedEntry<Thing, DbContext> e) => e.Entity.DeleteFailed = true;
		private static void DeleteFailedCheckFlags(IChangeFailedEntry<Thing, DbContext> e) => CheckFlags(e.Entity, nameof(e.Entity.Deleting), nameof(e.Entity.DeleteFailed));
		private static void DeletedTrue           (IAfterChangeEntry<Thing, DbContext>  e) => e.Entity.Deleted = true;
		private static void DeletedCheckFlags     (IAfterChangeEntry<Thing, DbContext>  e) => CheckFlags(e.Entity, nameof(e.Entity.Deleting), nameof(e.Entity.Deleted));

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

	public class Insert : ThingTestBase {
		[Fact]
		public void Sync() => DoATest(() => {
			Context.Things.Add(new Thing {Value = "Foo"});
			Context.SaveChanges();
		});

#if !NET40
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			Context.Things.Add(new Thing {Value = "Foo"});
			await Context.SaveChangesAsync();
		});
#endif
	}

	public class InsertFail : ThingTestBase {
		[Fact]
		public void Sync() => DoATest(() => {
			Context.Things.Add(new Thing { Value = null });
			try {
				Context.SaveChanges();
			}
#if EF_CORE
			catch (DbUpdateException) {
#else
			catch (DbEntityValidationException) {
#endif
			}
		});

#if !NET40
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			Context.Things.Add(new Thing { Value = null });
			try {
				await Context.SaveChangesAsync();
			}
#if EF_CORE
			catch (DbUpdateException) {
#else
			catch (DbEntityValidationException) {
#endif
			}
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
		});

#if !NET40
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var thing = new Thing { Value = "Foo" };
			Context.Things.Add(thing);
			await Context.SaveChangesAsync();
			thing.Value = "Bar";
			ResetFlags(thing);
			await Context.SaveChangesAsync();
		});
#endif
	}

	public class UpdateFail : ThingTestBase {
		[Fact]
		public void Sync() => DoATest(() => {
			var thing = new Thing { Value = "Foo" };
			Context.Things.Add(thing);
			Context.SaveChanges();
			thing.Value = "Bar";
			ResetFlags(thing);
			try {
				Context.SaveChanges();
			}
#if EF_CORE
			catch (DbUpdateException) {
#else
			catch (DbEntityValidationException) {
#endif
			}
		});

#if !NET40
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var thing = new Thing { Value = "Foo" };
			Context.Things.Add(thing);
			await Context.SaveChangesAsync();
			thing.Value = "Bar";
			ResetFlags(thing);
			try {
				await Context.SaveChangesAsync();
			}
#if EF_CORE
			catch (DbUpdateException) {
#else
			catch (DbEntityValidationException) {
#endif
			}
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
		});

#if !NET40
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var thing = new Thing { Value = "Foo" };
			Context.Things.Add(thing);
			await Context.SaveChangesAsync();
			ResetFlags(thing);
			Context.Things.Remove(thing);
			await Context.SaveChangesAsync();
		});
#endif
	}

	public class DeleteFail : ThingTestBase {
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
#if EF_CORE
			catch (DbUpdateException) {
#else
			catch (DbEntityValidationException) {
#endif
			}
		});

#if !NET40
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var thing = new Thing { Value = "Foo" };
			Context.Things.Add(thing);
			await Context.SaveChangesAsync();
			ResetFlags(thing);
			Context.Things.Remove(thing);
			try {
				await Context.SaveChangesAsync();
			}
#if EF_CORE
			catch (DbUpdateException) {
#else
			catch (DbEntityValidationException) {
#endif
			}
		});
#endif
	}

	public class CancelInserting : TestBase {
		protected override void Setup() {
			Triggers<Person>.Inserting += Cancel;
		}

		protected override void Teardown() {
			Triggers<Person>.Inserting -= Cancel;
		}

		private static void Cancel(IBeforeEntry<Person, DbContext> e) => e.Cancel();

		[Fact]
		public void Sync() => DoATest(() => {
			var guid = Guid.NewGuid().ToString();
			var person = new Person {LastName = guid};
			Context.People.Add(person);
			Context.SaveChanges();
			Assert.True(Context.People.SingleOrDefault(x => x.LastName == guid) == null);
		});

#if !NET40
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var guid = Guid.NewGuid().ToString();
			var person = new Person { LastName = guid };
			Context.People.Add(person);
			await Context.SaveChangesAsync();
			Assert.True(await Context.People.SingleOrDefaultAsync(x => x.LastName == guid) == null);
		});
#endif
	}

	public class CancelUpdating : TestBase {
		protected override void Setup() {
			Triggers<Person>.Updating += Cancel;
		}

		protected override void Teardown() {
			Triggers<Person>.Updating -= Cancel;
		}

		private static void Cancel(IBeforeChangeEntry<Person, DbContext> e) => e.Cancel();

		[Fact]
		public void Sync() => DoATest(() => {
			var guid = Guid.NewGuid().ToString();
			var person = new Person { LastName = guid };
			Context.People.Add(person);
			Context.SaveChanges();
			var updatedGuid = Guid.NewGuid().ToString();
			person.LastName = updatedGuid;
			Context.SaveChanges();
			Assert.True(Context.People.SingleOrDefault(x => x.LastName == guid) != null);
		});

#if !NET40
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var guid = Guid.NewGuid().ToString();
			var person = new Person {LastName = guid};
			Context.People.Add(person);
			await Context.SaveChangesAsync();
			var updatedGuid = Guid.NewGuid().ToString();
			person.LastName = updatedGuid;
			await Context.SaveChangesAsync();
			Assert.True(await Context.People.SingleOrDefaultAsync(x => x.LastName == guid) != null);
		});
#endif
	}

	public class CancelDeleting : TestBase {
		protected override void Setup() {
			Triggers<Person>.Deleting += Cancel;
		}

		protected override void Teardown() {
			Triggers<Person>.Deleting -= Cancel;
		}

		private static void Cancel(IBeforeChangeEntry<Person, DbContext> e) => e.Cancel();

		[Fact]
		public void Sync() => DoATest(() => {
			var guid = Guid.NewGuid().ToString();
			var person = new Person { LastName = guid };
			Context.People.Add(person);
			Context.SaveChanges();
			Context.People.Remove(person);
			Context.SaveChanges();
			Assert.True(Context.People.SingleOrDefault(x => x.LastName == guid) != null);
		});

#if !NET40
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var guid = Guid.NewGuid().ToString();
			var person = new Person { LastName = guid };
			Context.People.Add(person);
			await Context.SaveChangesAsync();
			Context.People.Remove(person);
			await Context.SaveChangesAsync();
			Assert.True(await Context.People.SingleOrDefaultAsync(x => x.LastName == guid) != null);
		});
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
			await Context.SaveChangesAsync();
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
			await Context.SaveChangesAsync();
			Assert.True(royalGala.Numbers.SequenceEqual(new[] { 1, 2, 3 }));
		});
#endif
	}
}