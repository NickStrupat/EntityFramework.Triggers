using System;
using System.Linq;
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

	public class UnitTests {
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

		// Cancel property of "before" trigger
		// Swallow proprety of "failed" trigger

		// TODO:
		// event loops
		// calling savechanges in an event handler
		// doubly-declared interfaces

		// test ...edFailed exception logic...
		//     DbUpdateException raises failed triggers and is swallowable if contains entries or changetracker has only one entry
		//     DbEntityValidationException raises failed triggers and is swallowable if it contains entries
		//     All other exceptions raises failed triggers and is swallowable if changetracker has only one entry
	}

	public class TriggersEnabled : TestBase {
		protected override void Setup() => Triggers<Thing>.Inserting += OnTriggersOnInserting;
		protected override void Teardown() => Triggers<Thing>.Inserting -= OnTriggersOnInserting;

		private void OnTriggersOnInserting(IInsertingEntry<Thing, DbContext> e) => e.Entity.Value = "changed";

		[Fact]
		public void Sync() => DoATest(() => {
			var thing = new Thing {Value = "okay"};
			Context.Things.Add(thing);
			Assert.True(Context.TriggersEnabled);
			Context.TriggersEnabled = false;
			Context.SaveChanges();
			Assert.True(thing.Value == "okay");

			var thing2 = new Thing {Value = "yep"};
			Context.Things.Add(thing2);
			Context.TriggersEnabled = true;
			Context.SaveChanges();
			Assert.True(thing2.Value == "changed");
		});
        
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var thing = new Thing { Value = "okay" };
			Context.Things.Add(thing);
			Assert.True(Context.TriggersEnabled);
			Context.TriggersEnabled = false;
			await Context.SaveChangesAsync().ConfigureAwait(false);
			Assert.True(thing.Value == "okay");

			var thing2 = new Thing { Value = "yep" };
			Context.Things.Add(thing2);
			Context.TriggersEnabled = true;
			await Context.SaveChangesAsync().ConfigureAwait(false);
			Assert.True(thing2.Value == "changed");
		});
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
        
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var guid = Guid.NewGuid().ToString();
			var thing = new Thing { Value = guid };
			Context.Things.Add(thing);
			await Context.SaveChangesAsync().ConfigureAwait(false);
			InsertedCheckFlags(thing);
			Assert.True(await Context.Things.SingleOrDefaultAsync(x => x.Value == guid).ConfigureAwait(false) != null);
		});
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
        
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			Context.Things.Add(new Thing { Value = null });
			await Context.SaveChangesAsync().ConfigureAwait(false);
		});
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
	}

	public class UpdateFailSwallow : TestBase {
		protected override void Setup() => Triggers<Thing>.UpdateFailed += OnUpdateFailed;
		protected override void Teardown() => Triggers<Thing>.UpdateFailed -= OnUpdateFailed;

		private static void OnUpdateFailed(IFailedEntry<Thing, DbContext> e) {
#if EF_CORE
			Assert.True(e.Exception is DbUpdateException);
#else
			Assert.True(e.Exception is DbEntityValidationException);
#endif
			e.Swallow = true;
		}

		[Fact]
		public void Sync() => DoATest(() => {
			var thing = new Thing { Value = "Foo" };
			Context.Things.Add(thing);
			Context.SaveChanges();
			thing.Value = null;
			Context.SaveChanges();
		});
        
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var thing = new Thing { Value = "Foo" };
			Context.Things.Add(thing);
			await Context.SaveChangesAsync().ConfigureAwait(false);
			thing.Value = null;
			await Context.SaveChangesAsync().ConfigureAwait(false);
		});
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

		private static void OnDeleting(IBeforeChangeEntry<Thing> e) {
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
	}

	public class DeleteFailSwallow : TestBase {
		protected override void Setup() {
			Triggers<Thing>.Deleting += OnDeleting;
			Triggers<Thing>.DeleteFailed += OnDeleteFailed;
		}

		protected override void Teardown() {
			Triggers<Thing>.DeleteFailed -= OnDeleteFailed;
			Triggers<Thing>.Deleting -= OnDeleting;
		}

		private static void OnDeleting(IBeforeChangeEntry<Thing, DbContext> e) {
			throw new Exception();
		}

		private static void OnDeleteFailed(IChangeFailedEntry<Thing, DbContext> e) {
			e.Swallow = true;
		}

		[Fact]
		public void Sync() => DoATest(() => {
			var thing = new Thing { Value = "Foo" };
			Context.Things.Add(thing);
			Context.SaveChanges();
			Context.Things.Remove(thing);
			Context.SaveChanges();
		});
        
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var thing = new Thing { Value = "Foo" };
			Context.Things.Add(thing);
			await Context.SaveChangesAsync().ConfigureAwait(false);
			Context.Things.Remove(thing);
			await Context.SaveChangesAsync().ConfigureAwait(false);
		});
	}

	public class InsertingCancel : ThingTestBase {
		protected override void Setup() {
			base.Setup();
			Triggers<Thing>.Inserting += Cancel;
			Triggers<Thing, Context>.Inserting += Cancel2; // <-- Note the specified Context class (the `Cancel` property must persist across 
		}
		protected override void Teardown() {
			Triggers<Thing, Context>.Inserting -= Cancel2;
			Triggers<Thing>.Inserting -= Cancel;
			base.Teardown();
		}

		protected virtual void Cancel(IBeforeEntry<Thing> e) => e.Cancel = true;

		private Boolean cancel2Ran;
		protected void Cancel2(IBeforeEntry<Thing> e) {
			cancel2Ran = true;
			Assert.True(e.Cancel, nameof(e.Cancel) + ": " + e.Cancel);
		}

		[Fact]
		public void Sync() => DoATest(() => {
			var guid = Guid.NewGuid().ToString();
			var thing = new Thing { Value = guid };
			Context.Things.Add(thing);
			Assert.False(cancel2Ran, nameof(cancel2Ran) + ": " + cancel2Ran);
			Context.SaveChanges();
			InsertingCheckFlags(thing);
			Assert.True(cancel2Ran, nameof(cancel2Ran) + ": " + cancel2Ran);
			Assert.True(Context.Things.SingleOrDefault(x => x.Value == guid) == null);
		});
        
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var guid = Guid.NewGuid().ToString();
			var thing = new Thing { Value = guid };
			Context.Things.Add(thing);
			Assert.False(cancel2Ran, nameof(cancel2Ran) + ": " + cancel2Ran);
			await Context.SaveChangesAsync().ConfigureAwait(false);
			InsertingCheckFlags(thing);
			Assert.True(cancel2Ran, nameof(cancel2Ran) + ": " + cancel2Ran);
			Assert.True(await Context.Things.SingleOrDefaultAsync(x => x.Value == guid).ConfigureAwait(false) == null);
		});
	}

	public class UpdatingCancel : ThingTestBase {
		protected override void Setup() {
			base.Setup();
			Triggers<Thing>.Updating += Cancel;
			Triggers<Thing, Context>.Updating += Cancel2; // <-- Note the specified Context class (the `Cancel` property must persist across 
		}
		protected override void Teardown() {
			Triggers<Thing, Context>.Updating -= Cancel2;
			Triggers<Thing>.Updating -= Cancel;
			base.Teardown();
		}

		protected virtual void Cancel(IBeforeChangeEntry<Thing, DbContext> e) => e.Cancel = true;

		private Boolean cancel2Ran;
		protected void Cancel2(IBeforeEntry<Thing> e) {
			cancel2Ran = true;
			Assert.True(e.Cancel, nameof(e.Cancel) + ": " + e.Cancel);
		}

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
            Assert.True(cancel2Ran);
			Assert.True(Context.Things.SingleOrDefault(x => x.Value == guid) != null);
			Assert.True(Context.Things.SingleOrDefault(x => x.Value == updatedGuid) == null);
		});
        
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
            Assert.True(cancel2Ran);
			Assert.True(await Context.Things.SingleOrDefaultAsync(x => x.Value == guid).ConfigureAwait(false) != null);
			Assert.True(await Context.Things.SingleOrDefaultAsync(x => x.Value == updatedGuid).ConfigureAwait(false) == null);
		});
	}

	public class DeletingCancel : ThingTestBase {
		protected override void Setup() {
			base.Setup();
			Triggers<Thing>.Deleting += Cancel;
			Triggers<Thing, Context>.Updating += Cancel2; // <-- Note the specified Context class (the `Cancel` property must persist across 
		}
		protected override void Teardown() {
			Triggers<Thing, Context>.Updating -= Cancel2;
			Triggers<Thing>.Deleting -= Cancel;
			base.Teardown();
		}

		protected virtual void Cancel(IBeforeChangeEntry<Thing, DbContext> e) => e.Cancel = true;

		private Boolean cancel2Ran;
		protected void Cancel2(IBeforeEntry<Thing> e) {
			cancel2Ran = true;
			Assert.True(e.Cancel, nameof(e.Cancel) + ": " + e.Cancel);
		}

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
            Assert.False(cancel2Ran);
			Assert.True(Context.Things.SingleOrDefault(x => x.Value == guid) != null);
		});
        
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
            Assert.False(cancel2Ran);
			Assert.True(await Context.Things.SingleOrDefaultAsync(x => x.Value == guid).ConfigureAwait(false) != null);
		});
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
        
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var thing = new Thing { Value = Guid.NewGuid().ToString() };
			Context.Things.Add(thing);
			await Context.SaveChangesAsync().ConfigureAwait(false);
			Assert.True(thing.Numbers.SequenceEqual(new[] { 1, 2, 3 }));
		});
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
        
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var royalGala = new RoyalGala { Value = Guid.NewGuid().ToString() };
			Context.RoyalGalas.Add(royalGala);
			await Context.SaveChangesAsync().ConfigureAwait(false);
			Assert.True(royalGala.Numbers.SequenceEqual(new[] { 1, 2, 3 }));
		});
	}

	public class EventFiringOrderRelativeToClassInterfaceAndDbContextHierarchy : TestBase {
		protected override void Setup() {
			Triggers<IThing    , DbContext>.Inserting += Add1;
			Triggers<Thing     , DbContext>.Inserting += Add2;
			Triggers<IApple    , DbContext>.Inserting += Add3;
			Triggers<Apple     , DbContext>.Inserting += Add4;
			Triggers<IRoyalGala, DbContext>.Inserting += Add5;
			Triggers<RoyalGala , DbContext>.Inserting += Add6;
			
			Triggers<IThing    , DbContextWithTriggers>.Inserting += Add7 ;
			Triggers<Thing     , DbContextWithTriggers>.Inserting += Add8 ;
			Triggers<IApple    , DbContextWithTriggers>.Inserting += Add9 ;
			Triggers<Apple     , DbContextWithTriggers>.Inserting += Add10;
			Triggers<IRoyalGala, DbContextWithTriggers>.Inserting += Add11;
			Triggers<RoyalGala , DbContextWithTriggers>.Inserting += Add12;
			
			Triggers<IThing    , Context>.Inserting += Add13;
			Triggers<Thing     , Context>.Inserting += Add14;
			Triggers<IApple    , Context>.Inserting += Add15;
			Triggers<Apple     , Context>.Inserting += Add16;
			Triggers<IRoyalGala, Context>.Inserting += Add17;
			Triggers<RoyalGala , Context>.Inserting += Add18;
		}

		protected override void Teardown() {
			Triggers<IThing    , DbContext>.Inserting -= Add1;
			Triggers<Thing     , DbContext>.Inserting -= Add2;
			Triggers<IApple    , DbContext>.Inserting -= Add3;
			Triggers<Apple     , DbContext>.Inserting -= Add4;
			Triggers<IRoyalGala, DbContext>.Inserting -= Add5;
			Triggers<RoyalGala , DbContext>.Inserting -= Add6;
			
			Triggers<IThing    , DbContextWithTriggers>.Inserting -= Add7 ;
			Triggers<Thing     , DbContextWithTriggers>.Inserting -= Add8 ;
			Triggers<IApple    , DbContextWithTriggers>.Inserting -= Add9 ;
			Triggers<Apple     , DbContextWithTriggers>.Inserting -= Add10;
			Triggers<IRoyalGala, DbContextWithTriggers>.Inserting -= Add11;
			Triggers<RoyalGala , DbContextWithTriggers>.Inserting -= Add12;
			
			Triggers<IThing    , Context>.Inserting -= Add13;
			Triggers<Thing     , Context>.Inserting -= Add14;
			Triggers<IApple    , Context>.Inserting -= Add15;
			Triggers<Apple     , Context>.Inserting -= Add16;
			Triggers<IRoyalGala, Context>.Inserting -= Add17;
			Triggers<RoyalGala , Context>.Inserting -= Add18;
		}

		private static void Add1(IInsertingEntry<IThing,     DbContext> e)      => e.Entity.Numbers.Add(1);
		private static void Add2(IInsertingEntry<IThing,     DbContext> e)      => e.Entity.Numbers.Add(2);
		private static void Add3(IInsertingEntry<IApple,     DbContext> e)      => e.Entity.Numbers.Add(3);
		private static void Add4(IInsertingEntry<IThing,     DbContext> e)      => e.Entity.Numbers.Add(4);
		private static void Add5(IInsertingEntry<IRoyalGala, DbContext> e)      => e.Entity.Numbers.Add(5);
		private static void Add6(IInsertingEntry<IThing,     DbContext> e)      => e.Entity.Numbers.Add(6);

		private static void Add7 (IInsertingEntry<IThing,     DbContextWithTriggers> e)      => e.Entity.Numbers.Add(7);
		private static void Add8 (IInsertingEntry<IThing,     DbContextWithTriggers> e)      => e.Entity.Numbers.Add(8);
		private static void Add9 (IInsertingEntry<IApple,     DbContextWithTriggers> e)      => e.Entity.Numbers.Add(9);
		private static void Add10(IInsertingEntry<IThing,     DbContextWithTriggers> e)      => e.Entity.Numbers.Add(10);
		private static void Add11(IInsertingEntry<IRoyalGala, DbContextWithTriggers> e)      => e.Entity.Numbers.Add(11);
		private static void Add12(IInsertingEntry<IThing,     DbContextWithTriggers> e)      => e.Entity.Numbers.Add(12);

		private static void Add13(IInsertingEntry<IThing,     Context> e)      => e.Entity.Numbers.Add(13);
		private static void Add14(IInsertingEntry<IThing,     Context> e)      => e.Entity.Numbers.Add(14);
		private static void Add15(IInsertingEntry<IApple,     Context> e)      => e.Entity.Numbers.Add(15);
		private static void Add16(IInsertingEntry<IThing,     Context> e)      => e.Entity.Numbers.Add(16);
		private static void Add17(IInsertingEntry<IRoyalGala, Context> e)      => e.Entity.Numbers.Add(17);
		private static void Add18(IInsertingEntry<IThing,     Context> e)      => e.Entity.Numbers.Add(18);

		[Fact]
		public void Sync() => DoATest(() => {
			var royalGala = new RoyalGala { Value = Guid.NewGuid().ToString() };
			Context.RoyalGalas.Add(royalGala);
			Context.SaveChanges();
			Assert.True(royalGala.Numbers.SequenceEqual(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 ,16, 17, 18 }));
		});
        
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var royalGala = new RoyalGala { Value = Guid.NewGuid().ToString() };
			Context.RoyalGalas.Add(royalGala);
			await Context.SaveChangesAsync().ConfigureAwait(false);
			Assert.True(royalGala.Numbers.SequenceEqual(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 }));
		});
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
	}

	public class OriginalValuesOnDeleting : TestBase {
		protected override void Setup()    => Triggers<Thing>.Deleting += TriggersOnDeleting;
		protected override void Teardown() => Triggers<Thing>.Deleting -= TriggersOnDeleting;

		private void TriggersOnDeleting(IBeforeChangeEntry<Thing, DbContext> beforeChangeEntry) {
			Assert.True(guid != null);
			Assert.True(guid2 != null);
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
	}

	public class Covariance : TestBase {
		protected override void Setup() {
			Action<IBeforeEntry<Thing, Context>> triggersOnInserting = entry => { }; // These two will break at runtime without the `CoContra` event-backing-field
			Action<IBeforeEntry<Thing, DbContext>> triggersOnInserting2 = entry => { };
			Triggers<Thing, Context>.Inserting += triggersOnInserting;
			Triggers<Thing, Context>.Inserting += triggersOnInserting2;

			Action<IBeforeEntry<Thing>> triggersOnInserting3 = entry => { };
			Action<IBeforeEntry<Thing, Context>> triggersOnInserting4 = entry => { };
			Action<IBeforeEntry<Object>> triggersOnInserting5 = entry => { };
			Triggers<Thing>.Inserting += triggersOnInserting3;
			Triggers<Thing>.Inserting += triggersOnInserting5;
			Triggers<Object>.Inserting += triggersOnInserting5;
			Triggers<Thing, Context>.Inserting += triggersOnInserting4;

			Triggers<Thing, Context>.Inserting += ObjectInserting6;
			Triggers<Thing, Context>.Inserting += ObjectInserting5;
			Triggers<Thing, Context>.Inserting += ObjectInserting4;
			Triggers<Thing, Context>.Inserting += ObjectInserting3;
			Triggers<Thing, Context>.Inserting += ObjectInserting2;
			Triggers<Thing, Context>.Inserting += ObjectInserting;
			Triggers<Thing, Context>.Inserting += ThingInserting6;
			Triggers<Thing, Context>.Inserting += ThingInserting5;
			Triggers<Thing, Context>.Inserting += ThingInserting4;
			Triggers<Thing, Context>.Inserting += ThingInserting3;
			Triggers<Thing, Context>.Inserting += ThingInserting2;
			Triggers<Thing, Context>.Inserting += ThingInserting;
		}

		protected override void Teardown() {
			Triggers<Thing, Context>.Inserting += ThingInserting;
			Triggers<Thing, Context>.Inserting += ThingInserting2;
			Triggers<Thing, Context>.Inserting += ThingInserting3;
			Triggers<Thing, Context>.Inserting += ThingInserting4;
			Triggers<Thing, Context>.Inserting += ThingInserting5;
			Triggers<Thing, Context>.Inserting += ThingInserting6;
			Triggers<Thing, Context>.Inserting += ObjectInserting;
			Triggers<Thing, Context>.Inserting += ObjectInserting2;
			Triggers<Thing, Context>.Inserting += ObjectInserting3;
			Triggers<Thing, Context>.Inserting += ObjectInserting4;
			Triggers<Thing, Context>.Inserting += ObjectInserting5;
			Triggers<Thing, Context>.Inserting += ObjectInserting6;
		}

		private Boolean thingInsertingRan;
		private Boolean thingInserting2Ran;
		private Boolean thingInserting3Ran;
		private Boolean objectInsertingRan;
		private Boolean objectInserting2Ran;
		private Boolean objectInserting3Ran;

		private void ThingInserting(IBeforeEntry<Thing, Context> entry) => thingInsertingRan = true;
		private void ThingInserting2(IBeforeEntry<Thing, DbContext> entry) => thingInserting2Ran = true;
		private void ThingInserting3(IBeforeEntry<Thing> entry) => thingInserting3Ran = true;
		private void ThingInserting4(IEntry<Thing, Context> entry)   {}
		private void ThingInserting5(IEntry<Thing, DbContext> entry) {}
		private void ThingInserting6(IEntry<Thing> entry)            {}
		private void ObjectInserting(IBeforeEntry<Object, Context> beforeEntry) => objectInsertingRan = true;
		private void ObjectInserting2(IBeforeEntry<Object, DbContext> beforeEntry) => objectInserting2Ran = true;
		private void ObjectInserting3(IBeforeEntry<Object> beforeEntry) => objectInserting3Ran = true;
		private void ObjectInserting4(IEntry<Object, Context> beforeEntry)   {}
		private void ObjectInserting5(IEntry<Object, DbContext> beforeEntry) {}
		private void ObjectInserting6(IEntry<Object> beforeEntry)            {}

		[Fact]
		public void Sync() => DoATest(() => {
			var guid = Guid.NewGuid().ToString();
			var thing = new Thing { Value = guid };
			Context.Things.Add(thing);
			Assert.False(thingInsertingRan);
			Assert.False(thingInserting2Ran);
			Assert.False(thingInserting3Ran);
			Assert.False(objectInsertingRan);
			Assert.False(objectInserting2Ran);
			Assert.False(objectInserting3Ran);
			Context.SaveChanges();
			Assert.True(Context.Things.SingleOrDefault(x => x.Value == guid) != null);
			Assert.True(thingInsertingRan);
			Assert.True(thingInserting2Ran);
			Assert.True(thingInserting3Ran);
			Assert.True(objectInsertingRan);
			Assert.True(objectInserting2Ran);
			Assert.True(objectInserting3Ran);
		});
        
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var guid = Guid.NewGuid().ToString();
			var thing = new Thing { Value = guid };
			Context.Things.Add(thing);
			Assert.False(thingInsertingRan);
			Assert.False(thingInserting2Ran);
			Assert.False(thingInserting3Ran);
			Assert.False(objectInsertingRan);
			Assert.False(objectInserting2Ran);
			Assert.False(objectInserting3Ran);
			await Context.SaveChangesAsync().ConfigureAwait(false);
			Assert.True(await Context.Things.SingleOrDefaultAsync(x => x.Value == guid).ConfigureAwait(false) != null);
			Assert.True(thingInsertingRan);
			Assert.True(thingInserting2Ran);
			Assert.True(thingInserting3Ran);
			Assert.True(objectInsertingRan);
			Assert.True(objectInserting2Ran);
			Assert.True(objectInserting3Ran);
		});
	}

	//	public class MultiplyDeclaredInterfaces : TestBase {
	//		protected override void Setup() {}
	//		protected override void Teardown() { }

	//		[Fact]
	//		public void Sync() => DoATest(() => {
	//		});
    
	//		[Fact]
	//		public Task Async() => DoATestAsync(async () => {
	//		});
	//	}

	//	public interface ICreature { }
	//	public class Creature : ICreature {
	//		[Key]
	//		public virtual Int64 Id { get; protected set; }
	//		public virtual String Name { get; set; }
	//	}
	//	public class Dog : Creature, ICreature { }
}