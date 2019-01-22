using System;
using System.Collections.Generic;
using System.Text;
using EntityFrameworkCore.Triggers;
using Xunit;

#if EF_CORE
namespace EntityFrameworkCore.Triggers.Tests
#else
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
