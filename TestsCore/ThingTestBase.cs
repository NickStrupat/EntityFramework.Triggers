using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
				var failedAssertMessage = flagPropertyInfo.Name;

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
}