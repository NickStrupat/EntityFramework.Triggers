using System.Runtime.CompilerServices;

namespace EntityFrameworkTriggers {
	internal static class TriggersWeak<TTriggerable> where TTriggerable : class, ITriggerable<TTriggerable>, new() {
		public static readonly ConditionalWeakTable<TTriggerable, Triggers<TTriggerable>> ConditionalWeakTable = new ConditionalWeakTable<TTriggerable, Triggers<TTriggerable>>();
	}
}