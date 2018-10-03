using System;
using System.Collections.Generic;
using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;
using SimpleInjector;

namespace Testing
{
	public interface IInserted
	{
		DateTime Inserted { get; }
	}

	public class Base : IInserted
	{
		public DateTime Inserted { get; private set; }
		public DateTime? Updated { get; private set; }

		static Base()
		{
			Triggers<Base>.Inserting += entry => entry.Entity.Inserted = DateTime.UtcNow;
			Triggers<Base>.Updating += entry => entry.Entity.Updated = DateTime.UtcNow;
		}
	}

	public class Entity : Base
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
		private static IList<Type> GetDbContextInheritanceChain<TDbContext>() where TDbContext : DbContext
		{
			// from DbContext up to TDbContext
			var types = new List<Type>();
			Type type = typeof(TDbContext);
			while (typeof(DbContext).IsAssignableFrom(type))
			{
				types.Add(type);
				type = type.BaseType;
			}
			types.Reverse();
			return types;
		}

		static void Main(String[] args)
		{
			var ic = GetDbContextInheritanceChain<Context>();

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
}
