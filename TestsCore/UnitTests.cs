using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using EntityFrameworkCore.Triggers;

namespace Tests {
    [TestClass]
    public class UnitTests {
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
            TestOrder(context => context.SaveChanges());
        }
#if !NET40
        [TestMethod]
        public void TestAsynchronous() {
            TestEvents(context => context.SaveChangesAsync().Result);
            TestOrder(context => context.SaveChangesAsync().Result);
        }
#endif
        private void TestEvents(Func<Context, Int32> saveChangesAction) {
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
            using (var context = new Context()) {
	            if (context.Database.Exists()) {
		            context.Database.Delete();
					context.Database.Create();
	            }
	            var people = context.People.ToList();
                var nickStrupat = new Person {
                                                 FirstName = "Nick",
                                                 LastName = "Strupat",
                                             };
                AddHandlers(nickStrupat);
				nickStrupat.Triggers().Deleting += e => {
					e.Entity.IsMarkedDeleted = true;
					e.Cancel();
				};
                context.People.Add(nickStrupat);

	            var johnSmith = new Person {
                                               FirstName = "John",
                                               LastName = "Smith"
                                           };
                AddHandlers(johnSmith);
                context.People.Add(johnSmith);
                AssertNoEventsHaveFired();

                saveChangesAction(context);
				Assert.IsTrue(nickStrupat.Number == 42);
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
        private void AddHandlers(Person person) {
			person.Triggers().Inserting += e => ((Context)e.Context).Things.Add(new Thing { Value = "Insert trigger fired for " + e.Entity.FirstName });
			person.Triggers().Inserting += e => ++insertingFiredCount;
			person.Triggers().Inserting += e => e.Entity.LastName = "asdf";
            person.Triggers().Updating += e => ++updatingFiredCount;
            person.Triggers().Deleting += e => ++deletingFiredCount;
            person.Triggers().Inserted += e => ++insertedFiredCount;
            person.Triggers().Updated += e => ++updatedFiredCount;
            person.Triggers().Deleted += e => ++deletedFiredCount;
			person.Triggers().InsertFailed += e => ((Context)e.Context).Things.Add(new Thing { Value = "Insert failed for " + e.Entity.FirstName + " with exception message: " + e.Exception.Message });
			person.Triggers().InsertFailed += e => ++insertFailedFiredCount;
			person.Triggers().UpdateFailed += e => ((Context)e.Context).Things.Add(new Thing { Value = updateFailedThingValue = "Update failed for " + e.Entity.FirstName + " with exception message: " + e.Exception.Message });
			person.Triggers().UpdateFailed += e => ++updateFailedFiredCount;
			person.Triggers().DeleteFailed += e => ((Context)e.Context).Things.Add(new Thing { Value = "Delete failed for " + e.Entity.FirstName + " with exception message: " + e.Exception.Message });
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

        private void TestOrder(Func<Context, Int32> saveChangesAction) {
            var list = new List<Int32>();
            using (var context = new Context()) {
                var janeDoe = new Person {
                    FirstName = "Jane",
                    LastName = "Doe",
                };
                janeDoe.Triggers().Inserted += e => list.Add(0);
                ((EntityWithInsertTracking)janeDoe).Triggers().Inserted += e => list.Add(1);
                ((EntityWithTracking)janeDoe).Triggers().Inserted += e => list.Add(2);
                context.People.Add(janeDoe);
                saveChangesAction(context);
            }
            Assert.IsTrue(list.SequenceEqual(new [] {0, 1, 2}));
        }

	    [TestMethod]
	    public void InstanceAndStaticEventTest() {
		    var instanceFiredCount = 0;
		    var staticFiredCount = 0;
			var nick = new Person { FirstName = "Nick", LastName = "Strupat" };
			var john = new Person { FirstName = "John", LastName = "Smith" };
			nick.Triggers().Inserting += entry => instanceFiredCount++;
		    Triggers<Person>.Inserting += entry => staticFiredCount++;
		    using (var context = new Context()) {
			    context.People.Add(nick);
			    context.People.Add(john);
				context.SaveChanges();
		    }
			Assert.AreEqual(1, instanceFiredCount);
			Assert.AreEqual(2, staticFiredCount);
		}

		class OtherContext : DbContext { }

		[TestMethod]
		public void TypedContextEventTest() {
			var instanceFiredCount = 0;
			var staticFiredCount = 0;
			var nick = new Person { FirstName = "Nick", LastName = "Strupat" };
			var john = new Person { FirstName = "John", LastName = "Smith" };

			nick.Triggers<Person, Context>().Inserting += OnInserting;
			nick.Triggers<Person, Context>().Inserting += OnInserting;
			nick.Triggers<Person, Context>().Inserting += OnInserting;
			nick.Triggers<Person, Context>().Inserting += OnInserting;
			nick.Triggers<Person, Context>().Inserting -= OnInserting;
			nick.Triggers<Person, Context>().Inserting -= OnInserting;

			nick.Triggers().Inserting += entry => instanceFiredCount++;
			nick.Triggers<Person, Context>().Inserting += entry => instanceFiredCount++;
			nick.Triggers<Person, OtherContext>().Inserting += entry => instanceFiredCount++;
			Triggers<Person, Context>.Inserting += entry => {
			                                           entry.Context.Things.Add(new Thing { Value = "Hat" });
			                                           staticFiredCount++;
			                                       };
			Triggers<Person, OtherContext>.Inserting += entry => staticFiredCount++;

			var actions = new Action<IBeforeEntry<Person, Context>>[] {
				OnInsertingO, OnInsertingH, OnInsertingE, OnInsertingH, OnInsertingL, OnInsertingE, OnInsertingL, OnInsertingO, OnInsertingL
			};
			foreach (var action in actions)
				nick.Triggers<Person, Context>().Inserting += action;

			foreach (var action in new Action<IBeforeEntry<Person, Context>>[] { OnInsertingE, OnInsertingH, OnInsertingL, OnInsertingO, OnInsertingO })
				nick.Triggers<Person, Context>().Inserting -= action;
			nick.Triggers<Person, Context>().Inserting += OnInsertingO;
			
			using (var context = new Context()) {
				context.People.Add(nick);
				context.People.Add(john);
				context.SaveChanges();
			}

			Assert.AreEqual(2, instanceFiredCount);
			Assert.AreEqual(2, staticFiredCount);
			Assert.AreEqual(2, onInsertingCount);
			Assert.AreEqual("HELLO", hello);
		}

		private Int32 onInsertingCount;
		private void OnInserting(IBeforeEntry<Person, Context> beforeEntry) => onInsertingCount++;

		private String hello;
	    private void OnInsertingH(IBeforeEntry<Person, Context> beforeEntry) => hello += "H";
		private void OnInsertingE(IBeforeEntry<Person, Context> beforeEntry) => hello += "E";
		private void OnInsertingL(IBeforeEntry<Person, Context> beforeEntry) => hello += "L";
		private void OnInsertingO(IBeforeEntry<Person, Context> beforeEntry) => hello += "O";

		[TestMethod]
	    public void OriginalValuesTest() {
			Person og = null;
			Triggers<Person, Context>.Updating += entry => og = entry.Original;
			using (var context = new Context()) {
				if (context.Database.Delete())
					context.Database.Create();
				var nick = new Person { FirstName = "Nick", LastName = "Strupat" };
				context.People.Add(nick);
				context.SaveChanges();
				nick.FirstName = "Ned";
				nick.LastName = "Sputnik";
				context.SaveChanges();
				Assert.AreEqual("Nick", og.FirstName);
				Assert.AreEqual("Strupat", og.LastName);
			}
		}
	}
}