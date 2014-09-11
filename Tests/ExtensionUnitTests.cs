using System;
using System.Linq;
using EntityFrameworkTriggers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests {
	[TestClass]
	public class ExtensionUnitTests {
		private Int32 insertingFiredCount;
        private Int32 updatingFiredCount;
        private Int32 deletingFiredCount;
        private Int32 insertedFiredCount;
        private Int32 updatedFiredCount;
        private Int32 deletedFiredCount;
        [TestMethod]
        public void TestSynchronous() {
            TestEvents(context => context.SaveChangesWithTriggers());
        }
        [TestMethod]
        public void TestAsynchronous() {
            TestEvents(context => context.SaveChangesWithTriggersAsync().Result);
        }
        private void TestEvents(Func<SealedContext, Int32> saveChangesAction) {
            insertingFiredCount = 0;
            updatingFiredCount = 0;
            deletingFiredCount = 0;
            insertedFiredCount = 0;
            updatedFiredCount = 0;
            deletedFiredCount = 0;
            using (var context = new SealedContext()) {
                var nickStrupat = new TriggerablePerson {
                                                 FirstName = "Nick",
                                                 LastName = "Strupat",
                                             };
                AddHandlers(nickStrupat, context);
                context.People.Add(nickStrupat);

                var johnSmith = new TriggerablePerson {
                                               FirstName = "John",
                                               LastName = "Smith"
                                           };
                AddHandlers(johnSmith, context);
                context.People.Add(johnSmith);
                AssertNoEventsHaveFired();

                saveChangesAction(context);
                AssertInsertEventsHaveFired();
                Assert.IsTrue(context.Things.First().Value == "Insert trigger fired for Nick");

                nickStrupat.FirstName = "Nicholas";
                saveChangesAction(context);
                AssertUpdateEventsHaveFired();

                context.People.Remove(nickStrupat);
                context.People.Remove(johnSmith);
                saveChangesAction(context);
                AssertAllEventsHaveFired();

                context.Database.Delete();
            }
        }
        private void AddHandlers(TriggerablePerson person, SealedContext context) {
            person.Triggers().Inserting += e => {
                                    context.Things.Add(new TriggerableThing {Value = "Insert trigger fired for " + e.FirstName});
                                    ++insertingFiredCount;
                                };
            person.Triggers().Updating += e => ++updatingFiredCount;
            person.Triggers().Deleting += e => ++deletingFiredCount;
            person.Triggers().Inserted += e => ++insertedFiredCount;
            person.Triggers().Updated += e => ++updatedFiredCount;
            person.Triggers().Deleted += e => ++deletedFiredCount;
        }
        private void AssertAllEventsHaveFired() {
            Assert.AreEqual(insertingFiredCount, 2);
            Assert.AreEqual(updatingFiredCount, 1);
            Assert.AreEqual(deletingFiredCount, 2);
            Assert.AreEqual(insertedFiredCount, 2);
            Assert.AreEqual(updatedFiredCount, 1);
            Assert.AreEqual(deletedFiredCount, 2);
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
