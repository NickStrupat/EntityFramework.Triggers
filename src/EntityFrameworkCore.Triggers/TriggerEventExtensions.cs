using System;
using System.Threading.Tasks;

namespace EntityFrameworkCore.Triggers;

internal static class TriggerEventExtensions
{
	public static void Raise(this ITriggerEvent triggersEvent, Object entry) => ((ITriggerEventInternal)triggersEvent).Raise(entry);
	public static Task RaiseAsync(this ITriggerEvent triggersEvent, Object entry) => ((ITriggerEventInternal)triggersEvent).RaiseAsync(entry);
}