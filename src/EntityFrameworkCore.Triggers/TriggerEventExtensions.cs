using System;

#if EF_CORE
namespace EntityFrameworkCore.Triggers
#else
namespace EntityFramework.Triggers
#endif
{
	internal static class TriggerEventExtensions
	{
		public static void Raise(this ITriggerEvent triggersEvent, Object entry) => ((ITriggerEventInternal)triggersEvent).Raise(entry);
	}
}