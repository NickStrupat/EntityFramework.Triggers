using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using EntityFramework.Triggers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests {
	[TestClass]
	public class ExtensionUnitTests {
		private Int32 insertingFiredCount;
        private Int32 updatingFiredCount;
		private Int32 deletingFiredCount;
		private Int32 insertFailedFiredCount;
		private Int32 updateFailedFiredCount;
		private Int32 deleteFailedFiredCount;
        private Int32 insertedFiredCount;
        private Int32 updatedFiredCount;
		private Int32 deletedFiredCount;
		private String updateFailedThingValue;
        [TestMethod]
        public void TestSynchronous() {
            TestEvents(context => context.SaveChanges());
        }
        [TestMethod]
        public void TestAsynchronous() {
            TestEvents(context => context.SaveChangesAsync().Result);
        }
        private void TestEvents(Func<SealedContext, Int32> saveChangesAction) {
            insertingFiredCount = 0;
            updatingFiredCount = 0;
			deletingFiredCount = 0;
			insertFailedFiredCount = 0;
			updateFailedFiredCount = 0;
			deleteFailedFiredCount = 0;
            insertedFiredCount = 0;
            updatedFiredCount = 0;
            deletedFiredCount = 0;
	        updateFailedThingValue = null;
            using (var context = new SealedContext()) {
                var nickStrupat = new TriggerablePerson {
                                                 FirstName = "Nick",
                                                 LastName = "Strupat",
                                             };
				AddHandlers(nickStrupat);
				nickStrupat.Triggers().Deleting += e => {
					e.Entity.IsMarkedDeleted = true;
					e.Context.Entry(e.Entity).State = EntityState.Modified;
				};
                context.People.Add(nickStrupat);

                var johnSmith = new TriggerablePerson {
                                               FirstName = "John",
                                               LastName = "Smith"
                                           };
                AddHandlers(johnSmith);
                context.People.Add(johnSmith);
                AssertNoEventsHaveFired();

                saveChangesAction(context);
                AssertInsertEventsHaveFired();
                Assert.IsTrue(context.Things.First().Value == "Insert trigger fired for Nick");

                nickStrupat.FirstName = "Nicholas";
                saveChangesAction(context);
				AssertUpdateEventsHaveFired();

				nickStrupat.LastName = null;
				try {
					context.SaveChanges();
				}
				catch (DbEntityValidationException ex) {
					nickStrupat.LastName = "Strupat";
				}
				catch (Exception ex) {
					Assert.Fail(ex.GetType().Name + " exception caught");
				}
				context.SaveChanges();
				Assert.AreEqual(updateFailedFiredCount, 1);
				Assert.IsTrue(context.Things.OrderByDescending(x => x.Id).First().Value == updateFailedThingValue);

                context.People.Remove(nickStrupat);
                context.People.Remove(johnSmith);
                saveChangesAction(context);
                AssertAllEventsHaveFired();

                context.Database.Delete();
            }
        }
		private void AddHandlers(TriggerablePerson person) {
			person.Triggers().Inserting += e => e.GetContext<SealedContext>().Things.Add(new TriggerableThing { Value = "Insert trigger fired for " + e.Entity.FirstName });
			person.Triggers().Inserting += e => ++insertingFiredCount;
			person.Triggers().Updating += e => ++updatingFiredCount;
			person.Triggers().Deleting += e => ++deletingFiredCount;
			person.Triggers().Inserted += e => ++insertedFiredCount;
			person.Triggers().Updated += e => ++updatedFiredCount;
			person.Triggers().Deleted += e => ++deletedFiredCount;
			person.Triggers().InsertFailed += e => e.GetContext<SealedContext>().Things.Add(new TriggerableThing { Value = "Insert failed for " + e.Entity.FirstName + " with exception message: " + e.Exception.Message });
			person.Triggers().InsertFailed += e => ++insertFailedFiredCount;
			person.Triggers().UpdateFailed += e => e.GetContext<SealedContext>().Things.Add(new TriggerableThing { Value = updateFailedThingValue = "Update failed for " + e.Entity.FirstName + " with exception message: " + e.Exception.Message });
			person.Triggers().UpdateFailed += e => ++updateFailedFiredCount;
			person.Triggers().DeleteFailed += e => e.GetContext<SealedContext>().Things.Add(new TriggerableThing { Value = "Delete failed for " + e.Entity.FirstName + " with exception message: " + e.Exception.Message });
			person.Triggers().DeleteFailed += e => ++deleteFailedFiredCount;
        }
        private void AssertAllEventsHaveFired() {
            Assert.AreEqual(insertingFiredCount, 2);
            Assert.AreEqual(updatingFiredCount, 3);
            Assert.AreEqual(deletingFiredCount, 2);
            Assert.AreEqual(insertedFiredCount, 2);
            Assert.AreEqual(updatedFiredCount, 2);
            Assert.AreEqual(deletedFiredCount, 1);
        }
        private void AssertUpdateEventsHaveFired() {
            Assert.AreEqual(updatingFiredCount, 1);
            Assert.AreEqual(deletingFiredCount, 0);
            Assert.AreEqual(updatedFiredCount, 1);
            Assert.AreEqual(deletedFiredCount, 0);
        }
        private void AssertInsertEventsHaveFired() {
            Assert.AreEqual(insertingFiredCount, 2);
            Assert.AreEqual(updatingFiredCount, 0);
            Assert.AreEqual(deletingFiredCount, 0);
            Assert.AreEqual(insertedFiredCount, 2);
            Assert.AreEqual(updatedFiredCount, 0);
            Assert.AreEqual(deletedFiredCount, 0);
        }
        private void AssertNoEventsHaveFired() {
            Assert.AreEqual(insertingFiredCount, 0);
            Assert.AreEqual(updatingFiredCount, 0);
            Assert.AreEqual(deletingFiredCount, 0);
            Assert.AreEqual(insertedFiredCount, 0);
            Assert.AreEqual(updatedFiredCount, 0);
            Assert.AreEqual(deletedFiredCount, 0);
        }
    }
}
