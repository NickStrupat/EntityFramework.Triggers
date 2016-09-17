using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
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

		// TODO:
		// DbEntityValidationException
		// DbUpdateException

		// event order
		// original values on updating
		// inheritance hierarchy event order
		// doubly-declared interfaces
		// event loops
		// calling savechanges in an event handler
	}

	public abstract class TestBase : IDisposable {
		protected readonly Context Context = new Context();
		public void Dispose() => Context.Dispose();
	}

	public abstract class ThingTestBase : TestBase {
		static ThingTestBase() {
			Triggers<Thing>.Inserting    += e => e.Entity.Inserting    = true;
			Triggers<Thing>.Inserting    += e => CheckFlags(e.Entity, nameof(e.Entity.Inserting));

			Triggers<Thing>.InsertFailed += e => e.Entity.InsertFailed = true;
			Triggers<Thing>.InsertFailed += e => CheckFlags(e.Entity, nameof(e.Entity.Inserting), nameof(e.Entity.InsertFailed));

			Triggers<Thing>.Inserted     += e => e.Entity.Inserted     = true;
			Triggers<Thing>.Inserted     += e => CheckFlags(e.Entity, nameof(e.Entity.Inserting), nameof(e.Entity.Inserted));

			Triggers<Thing>.Updating     += e => e.Entity.Updating     = true;
			Triggers<Thing>.Updating     += e => CheckFlags(e.Entity, nameof(e.Entity.Updating));

			Triggers<Thing>.UpdateFailed += e => e.Entity.UpdateFailed = true;
			Triggers<Thing>.UpdateFailed += e => CheckFlags(e.Entity, nameof(e.Entity.Updating), nameof(e.Entity.UpdateFailed));

			Triggers<Thing>.Updated      += e => e.Entity.Updated      = true;
			Triggers<Thing>.Updated      += e => CheckFlags(e.Entity, nameof(e.Entity.Updating), nameof(e.Entity.Updated));

			Triggers<Thing>.Deleting     += e => e.Entity.Deleting     = true;
			Triggers<Thing>.Deleting     += e => CheckFlags(e.Entity, nameof(e.Entity.Deleting));

			Triggers<Thing>.DeleteFailed += e => e.Entity.DeleteFailed = true;
			Triggers<Thing>.DeleteFailed += e => CheckFlags(e.Entity, nameof(e.Entity.Deleting), nameof(e.Entity.DeleteFailed));

			Triggers<Thing>.Deleted      += e => e.Entity.Deleted      = true;
			Triggers<Thing>.Deleted      += e => CheckFlags(e.Entity, nameof(e.Entity.Deleting), nameof(e.Entity.Deleted));
		}

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
		public void Sync() {
			Context.Things.Add(new Thing {Value = "Foo"});
			Context.SaveChanges();
		}

#if !NET40
		[Fact]
		public async void Async() {
			Context.Things.Add(new Thing {Value = "Foo"});
			await Context.SaveChangesAsync();
		}
#endif
	}

	public class InsertFail : ThingTestBase {
		[Fact]
		public void Sync() {
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
		}

#if !NET40
		[Fact]
		public async void Async() {
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
		}
#endif
	}

	public class Update : ThingTestBase {
		[Fact]
		public void Sync() {
			var thing = new Thing { Value = "Foo" };
			Context.Things.Add(thing);
			Context.SaveChanges();
			thing.Value = "Bar";
			ResetFlags(thing);
			Context.SaveChanges();
		}

#if !NET40
		[Fact]
		public async void Async() {
			var thing = new Thing { Value = "Foo" };
			Context.Things.Add(thing);
			await Context.SaveChangesAsync();
			thing.Value = "Bar";
			ResetFlags(thing);
			await Context.SaveChangesAsync();
		}
#endif
	}

	public class UpdateFail : ThingTestBase {
		[Fact]
		public void Sync() {
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
		}

#if !NET40
		[Fact]
		public async void Async() {
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
		}
#endif
	}

	public class Delete : ThingTestBase {
		[Fact]
		public void Sync() {
			var thing = new Thing { Value = "Foo" };
			Context.Things.Add(thing);
			Context.SaveChanges();
			ResetFlags(thing);
			Context.Things.Remove(thing);
			Context.SaveChanges();
		}

#if !NET40
		[Fact]
		public async void Async() {
			var thing = new Thing { Value = "Foo" };
			Context.Things.Add(thing);
			await Context.SaveChangesAsync();
			ResetFlags(thing);
			Context.Things.Remove(thing);
			await Context.SaveChangesAsync();
		}
#endif
	}

	public class DeleteFail : ThingTestBase {
		[Fact]
		public void Sync() {
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
		}

#if !NET40
		[Fact]
		public async void Async() {
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
		}
#endif
	}

	public class CancelInserting : TestBase {
		static CancelInserting() {
			Triggers<Thing>.Inserting += e => e.Cancel();
		}

		[Fact]
		public void Sync() {
			var guid = Guid.NewGuid().ToString();
			var thing = new Thing { Value = guid };
			Context.Things.Add(thing);
			Context.SaveChanges();
			Assert.True(Context.Things.SingleOrDefault(x => x.Value == guid) == null);
		}

#if !NET40
		[Fact]
		public async void Async() {
			var guid = Guid.NewGuid().ToString();
			var thing = new Thing { Value = guid };
			Context.Things.Add(thing);
			await Context.SaveChangesAsync();
			Assert.True(await Context.Things.SingleOrDefaultAsync(x => x.Value == guid) == null);
		}
#endif
	}

	public class CancelUpdating : TestBase {
		static CancelUpdating() {
			Triggers<Thing>.Updating += e => e.Cancel();
		}

		[Fact]
		public void Sync() {
			var guid = Guid.NewGuid().ToString();
			var thing = new Thing { Value = guid };
			Context.Things.Add(thing);
			Context.SaveChanges();
			var updatedGuid = Guid.NewGuid().ToString();
			thing.Value = updatedGuid;
			Context.SaveChanges();
			Assert.True(Context.Things.SingleOrDefault(x => x.Value == guid) != null);
		}

#if !NET40
		[Fact]
		public async void Async() {
			var guid = Guid.NewGuid().ToString();
			var thing = new Thing { Value = guid };
			Context.Things.Add(thing);
			await Context.SaveChangesAsync();
			var updatedGuid = Guid.NewGuid().ToString();
			thing.Value = updatedGuid;
			await Context.SaveChangesAsync();
			Assert.True(await Context.Things.SingleOrDefaultAsync(x => x.Value == guid) != null);
		}
#endif
	}

	public class CancelDeleting : TestBase {
		static CancelDeleting() {
			Triggers<Thing>.Deleting += e => e.Cancel();
		}

		[Fact]
		public void Sync() {
			var guid = Guid.NewGuid().ToString();
			var thing = new Thing { Value = guid };
			Context.Things.Add(thing);
			Context.SaveChanges();
			Context.Things.Remove(thing);
			Context.SaveChanges();
			Assert.True(Context.Things.SingleOrDefault(x => x.Value == guid) != null);
		}

#if !NET40
		[Fact]
		public async void Async() {
			var guid = Guid.NewGuid().ToString();
			var thing = new Thing { Value = guid };
			Context.Things.Add(thing);
			await Context.SaveChangesAsync();
			Context.Things.Remove(thing);
			await Context.SaveChangesAsync();
			Assert.True(await Context.Things.SingleOrDefaultAsync(x => x.Value == guid) != null);
		}
#endif
	}
}