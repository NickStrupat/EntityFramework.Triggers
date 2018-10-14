using System;
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
		public Foo() => Count = instanceCount += 1;
	}

	public class Bar
	{
		private static Int32 instanceCount;
		public readonly Int32 Count;
		public Bar() => Count = instanceCount += 10;
	}

	public class Context : DbContextWithTriggers, IWhat
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


			Triggers<Entity>.Inserting += x => x.Entity.Name = "";
			using (var container = new Container())
			{
				container.Register<IServiceProvider>(() => container, Lifestyle.Singleton);
				container.Register<Context>(Lifestyle.Transient);
				container.Register<Foo>(Lifestyle.Transient);
				container.Register<Bar>(Lifestyle.Transient);
				container.Register(typeof(ITriggers<,>), typeof(Triggers<,>), Lifestyle.Singleton);
				container.Register(typeof(ITriggers<>), typeof(Triggers<>), Lifestyle.Singleton);
				container.Register(typeof(ITriggers), typeof(Triggers), Lifestyle.Singleton);

				var triggers = container.GetInstance<ITriggers<Entity, Context>>();
				var triggers1 = container.GetInstance<ITriggers<Entity>>();
				var triggers2 = container.GetInstance<ITriggers<Entity, DbContext>>();
				var triggers3 = container.GetInstance<ITriggers>();
				var triggers4 = container.GetInstance<ITriggers<Object, DbContext>>();
				var bb = triggers1.Equals(triggers2);
				var bbb = ReferenceEquals(triggers1, triggers2);
				var bbbb = triggers3.Equals(triggers4);
				var bbbbb = ReferenceEquals(triggers3, triggers4);
			    //triggers.Inserting.Add<Foo>((entry, foo) => entry.Entity.Inserted = DateTime.UtcNow);
				//triggers.Updating.Add<Foo>((entry, foo) => entry.Entity.Updated = DateTime.UtcNow);
				//triggers.Inserting.Add<Foo>((entry, foo) => entry.Entity.Name = foo.Count.ToString());
				Triggers<Entity, Context>.GlobalInserting.Add<(Foo Foo, Bar Bar)>(entry => Console.WriteLine(entry.Service.Foo.Count + " " + entry.Service.Bar.Count));
				Triggers<Entity, Context>.GlobalUpdating.Add<(Foo Foo, Bar Bar)>(entry => Console.WriteLine(entry.Service.Foo.Count + " " + entry.Service.Bar.Count));

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
}
