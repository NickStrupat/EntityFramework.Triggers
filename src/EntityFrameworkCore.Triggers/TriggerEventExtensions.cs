using System;
using System.Threading.Tasks;

#if EF_CORE
namespace EntityFrameworkCore.Triggers
#else
namespace EntityFramework.Triggers
#endif
{
	internal static class TriggerEventExtensions
	{
		public static void Raise(this ITriggerEvent triggersEvent, Object entry) => ((ITriggerEventInternal)triggersEvent).Raise(entry);
		public static Task RaiseAsync(this ITriggerEvent triggersEvent, Object entry) => ((ITriggerEventInternal)triggersEvent).RaiseAsync(entry);
	}
}