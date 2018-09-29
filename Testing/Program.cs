using System;
using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Testing
{
	public class Entity
	{
		public Int64 Id { get; private set; }
		public DateTime Inserted { get; set; }
		public DateTime? Updated { get; set; }
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
		static void Main(String[] args)
		{
            Triggers<Entity>.Inserting.Add(x => x.Entity.Name = "");
			using (var container = new Container())
			{
				container.Register<IServiceProvider>(() => container, Lifestyle.Singleton);
				container.Register<Context>(Lifestyle.Transient);
				container.Register<Foo>(Lifestyle.Transient);
				container.Register(typeof(ITriggers<,>), typeof(Triggers<,>), Lifestyle.Singleton);

				var triggers = container.GetInstance<ITriggers<Entity, Context>>();
				triggers.Inserting.Add<Foo>((entry, foo) => entry.Entity.Inserted = DateTime.UtcNow);
				triggers.Updating.Add<Foo>((entry, foo) => entry.Entity.Updated = DateTime.UtcNow);
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

	public static class TriggersExtensions
	{
        public static IServiceCollection AddDbContextWithTriggers(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton(typeof(Triggers<,>), typeof(Triggers<,>))
                                    .AddSingleton(typeof(Triggers<>), typeof(Triggers<>));
        }
    }
}
