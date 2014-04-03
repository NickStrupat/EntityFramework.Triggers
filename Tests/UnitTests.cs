using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests {
    [TestClass]
    public class UnitTests {
        private Int32 insertingFiredCount;
        private Int32 updatingFiredCount;
        private Int32 deletingFiredCount;
        private Int32 insertedFiredCount;
        private Int32 updatedFiredCount;
        private Int32 deletedFiredCount;
        [TestMethod]
        public void TestSynchronous() {
            TestEvents(context => context.SaveChanges());
        }
        [TestMethod]
        public void TestAsynchronous() {
            TestEvents(context => context.SaveChangesAsync().Result);
        }
        private void TestEvents(Func<Context, Int32> saveChangesAction) {
            insertingFiredCount = 0;
            updatingFiredCount = 0;
            deletingFiredCount = 0;
            insertedFiredCount = 0;
            updatedFiredCount = 0;
            deletedFiredCount = 0;
            using (var context = new Context()) {
                var nickStrupat = new Person {
                                                 FirstName = "Nick",
                                                 LastName = "Strupat",
                                             };
                AddHandlers(nickStrupat);
                context.People.Add(nickStrupat);

                var johnSmith = new Person {
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

                context.People.Remove(nickStrupat);
                context.People.Remove(johnSmith);
                saveChangesAction(context);
                AssertAllEventsHaveFired();

                context.Database.Delete();
            }
        }
        private void AddHandlers(Person person) {
            person.Inserting += (c, e) => {
                                    c.Things.Add(new Thing {Value = "Insert trigger fired for " + e.FirstName});
                                    ++insertingFiredCount;
                                };
            person.Updating += (c, e) => ++updatingFiredCount;
            person.Deleting += (c, e) => ++deletingFiredCount;
            person.Inserted += (c, e) => ++insertedFiredCount;
            person.Updated += (c, e) => ++updatedFiredCount;
            person.Deleted += (c, e) => ++deletedFiredCount;
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