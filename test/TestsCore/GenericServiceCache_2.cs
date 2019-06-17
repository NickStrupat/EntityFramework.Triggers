using System;
using Xunit;

#if EF_CORE
# if NETCOREAPP2_0
namespace EntityFrameworkCore.Triggers.Tests
# else
namespace EntityFrameworkCore.Triggers.Tests.Net461
# endif
#else
using System.Data.Entity;
using System.Data.Entity.Validation;
namespace EntityFramework.Triggers.Tests
#endif
{
	public class GenericServiceCacheTests
	{
		interface IFoo { }
		class Foo<T> : IFoo { }

		[Fact]
		public void Test()
		{
			var foo = GenericServiceCache<IFoo, Foo<Object>>.GetOrAdd(typeof(Int32));
			Assert.IsType<Foo<Int32>>(foo);
		}
	}
}
