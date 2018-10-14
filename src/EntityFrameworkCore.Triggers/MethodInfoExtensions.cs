using System;
using System.Reflection;

#if EF_CORE
namespace EntityFrameworkCore.Triggers
#else
namespace EntityFramework.Triggers
#endif
{
	public static class MethodInfoExtensions
	{
		public static TDelegate CreateDelegate<TDelegate>(this MethodInfo methodInfo)
		where TDelegate : Delegate =>
			(TDelegate) methodInfo.CreateDelegate(typeof(TDelegate));
	}
}
