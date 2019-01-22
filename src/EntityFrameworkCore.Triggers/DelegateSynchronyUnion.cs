using System;
using System.Threading.Tasks;

#if EF_CORE
namespace EntityFrameworkCore.Triggers
#else
namespace EntityFramework.Triggers
#endif
{
	internal struct DelegateSynchronyUnion<T>
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
	}
}