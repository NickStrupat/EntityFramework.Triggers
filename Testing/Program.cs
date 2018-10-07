using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;
using SimpleInjector;

namespace Testing
{
	public interface IInserted
	{
		DateTime Inserted { get; }
	}

	public class Base : IInserted, IDisposable
	{
		public DateTime Inserted { get; private set; }
		public DateTime? Updated { get; private set; }

		static Base()
		{
			Triggers<Base>.Inserting += entry => entry.Entity.Inserted = DateTime.UtcNow;
			Triggers<Base>.Updating += entry => entry.Entity.Updated = DateTime.UtcNow;
		}

		public void Dispose() {}
	}
	public interface IWhat { }
	public class Entity : Base, IInserted, IDisposable, IWhat
	{
		public Int64 Id { get; private set; }
		public String Name { get; set; }
	}

	public class Foo
	{
		private static Int32 instanceCount;
		public readonly Int32 Count;
		public Foo() => Count = instanceCount++;
	}

	public class Context : DbContextWithTriggers
	{
		public Context(IServiceProvider serviceProvider) : base(serviceProvider) {}
		//public Context(IServiceProvider serviceProvider, DbContextOptions options) : base(serviceProvider, options) {}

		public virtual DbSet<Entity> Entities { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
				optionsBuilder.UseInMemoryDatabase("what");
		}
	}

	class Program
	{
		static void Foo(Object o) {}

		static void Main(String[] args)
		{
			Action<String> aaa = (Action<String>)((Action<Object>)Foo).Method.CreateDelegate(typeof(Action<String>));

			IInsertingEntry<Entity, Context> asdf = new What<Context, Entity>.InsertingEntry(new Entity(), new Context(null), false);
			IInsertingEntry<Object, DbContext> asdf2 = asdf;

			var ok = TriggerEntityInvoker<Context, Entity>.GetRaiseActions<IInsertingEntry<Entity, Context>>("GlobalInserting", "Inserting");

			Triggers<Entity>.Inserting += x => x.Entity.Name = "";
			using (var container = new Container())
			{
				container.Register<IServiceProvider>(() => container, Lifestyle.Singleton);
				container.Register<Context>(Lifestyle.Transient);
				container.Register<Foo>(Lifestyle.Transient);
				container.Register(typeof(ITriggers<,>), typeof(Triggers<,>), Lifestyle.Singleton);
				container.Register(typeof(ITriggers<>), typeof(Triggers<>), Lifestyle.Singleton);

				var triggers = container.GetInstance<ITriggers<Entity, Context>>();
				var triggers1 = container.GetInstance<ITriggers<Entity>>();
				var triggers2 = container.GetInstance<ITriggers<Entity, DbContext>>();
				var bb = triggers1.Equals(triggers2);
				var bbb = ReferenceEquals(triggers1, triggers2);
			    //triggers.Inserting.Add<Foo>((entry, foo) => entry.Entity.Inserted = DateTime.UtcNow);
				//triggers.Updating.Add<Foo>((entry, foo) => entry.Entity.Updated = DateTime.UtcNow);
				triggers.Inserting.Add<Foo>((entry, foo) => entry.Entity.Name = foo.Count.ToString());

				using (var context = container.GetInstance<Context>())
				{
					var a = new Entity();
					var b = new Entity();
					context.Add(a);
					context.Add(b);
					context.SaveChanges();
					a.Name = "Test";
					context.SaveChanges();
				}
			}
		}
	}
	public class What<TDbContext, TEntity>
	where TDbContext : DbContext
	where TEntity : class
	{
		#region Entry implementations
		public abstract class Entry : IEntry<TEntity, TDbContext>
		{
			protected Entry(TEntity entity, TDbContext context)
			{
				Entity = entity;
				Context = context;
			}
			public TEntity Entity { get; }
			public TDbContext Context { get; }
			DbContext IEntry<TEntity>.Context => Context;
		}

		public abstract class BeforeEntry : Entry, IBeforeEntry<TEntity, TDbContext>
		{
			protected BeforeEntry(TEntity entity, TDbContext context, Boolean cancel) : base(entity, context)
			{
				Cancel = cancel;
			}
			public Boolean Cancel { get; set; }
		}

		public abstract class BeforeChangeEntry : BeforeEntry, IBeforeChangeEntry<TEntity, TDbContext>
		{
			protected BeforeChangeEntry(TEntity entity, TDbContext context, Boolean cancel) : base(entity, context, cancel) { }
			private TEntity original;
			public TEntity Original => original ?? (original = (TEntity)Context.Entry(Entity).OriginalValues.ToObject());
		}

		public abstract class FailedEntry : Entry, IFailedEntry<TEntity, TDbContext>
		{
			protected FailedEntry(TEntity entity, TDbContext context, Exception exception, Boolean swallow) : base(entity, context)
			{
				Exception = exception;
				Swallow = swallow;
			}
			public Exception Exception { get; }
			public Boolean Swallow { get; set; }
		}

		public abstract class ChangeFailedEntry : Entry, IChangeFailedEntry<TEntity, TDbContext>
		{
			public Exception Exception { get; }
			public Boolean Swallow { get; set; }

			protected ChangeFailedEntry(TEntity entity, TDbContext context, Exception exception, Boolean swallow) : base(entity, context)
			{
				Exception = exception;
				Swallow = swallow;
			}
		}


		public class InsertingEntry : BeforeEntry, IInsertingEntry<TEntity, TDbContext>
		{
			public InsertingEntry(TEntity entity, TDbContext context, Boolean cancel) : base(entity, context, cancel) { }
		}

		public class UpdatingEntry : BeforeChangeEntry, IUpdatingEntry<TEntity, TDbContext>
		{
			public UpdatingEntry(TEntity entity, TDbContext context, Boolean cancel) : base(entity, context, cancel) { }
		}

		public class DeletingEntry : BeforeChangeEntry, IDeletingEntry<TEntity, TDbContext>
		{
			public DeletingEntry(TEntity entity, TDbContext context, Boolean cancel) : base(entity, context, cancel) { }
		}

		public class InsertFailedEntry : FailedEntry, IInsertFailedEntry<TEntity, TDbContext>
		{
			public InsertFailedEntry(TEntity entity, TDbContext context, Exception exception, Boolean swallow) : base(entity, context, exception, swallow) { }
		}

		public class UpdateFailedEntry : ChangeFailedEntry, IUpdateFailedEntry<TEntity, TDbContext>
		{
			public UpdateFailedEntry(TEntity entity, TDbContext context, Exception exception, Boolean swallow) : base(entity, context, exception, swallow) { }
		}

		public class DeleteFailedEntry : ChangeFailedEntry, IDeleteFailedEntry<TEntity, TDbContext>
		{
			public DeleteFailedEntry(TEntity entity, TDbContext context, Exception exception, Boolean swallow) : base(entity, context, exception, swallow) { }
		}

		public class InsertedEntry : Entry, IInsertedEntry<TEntity, TDbContext>
		{
			public InsertedEntry(TEntity entity, TDbContext context) : base(entity, context) { }
		}

		public class UpdatedEntry : Entry, IUpdatedEntry<TEntity, TDbContext>
		{
			public UpdatedEntry(TEntity entity, TDbContext context) : base(entity, context) { }
		}

		public class DeletedEntry : Entry, IDeletedEntry<TEntity, TDbContext>
		{
			public DeletedEntry(TEntity entity, TDbContext context) : base(entity, context) { }
		}
		#endregion
	}
}
