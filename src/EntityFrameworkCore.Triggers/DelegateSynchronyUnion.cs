using System;
using System.Threading.Tasks;

#if EF_CORE
namespace EntityFrameworkCore.Triggers
#else
namespace EntityFramework.Triggers
#endif
{
	internal struct DelegateSynchronyUnion<T> : IEquatable<DelegateSynchronyUnion<T>>
	{
		private readonly Delegate @delegate;
		public DelegateSynchronyUnion(Action<T> action) => @delegate = action ?? throw new ArgumentNullException(nameof(action));
		public DelegateSynchronyUnion(Func<T, Task> func) => @delegate = func ?? throw new ArgumentNullException(nameof(func));

		public void Invoke(T value)
		{
			switch (@delegate)
			{
				case Action<T> action:
					action(value);
					break;
				case Func<T, Task> func:
					func(value).Wait();
					break;
			}
		}

		public async Task InvokeAsync(T value)
		{
			switch (@delegate)
			{
				case Action<T> action:
					action(value);
					break;
				case Func<T, Task> func:
					await func(value);
					break;
			}
		}

		public override Int32 GetHashCode() => @delegate.GetHashCode();
		public override String ToString() => @delegate.ToString();
		public override Boolean Equals(Object o) => o is DelegateSynchronyUnion<T> dsu && Equals(dsu);
		public Boolean Equals(DelegateSynchronyUnion<T> dsu) => @delegate == dsu.@delegate;
	}
}