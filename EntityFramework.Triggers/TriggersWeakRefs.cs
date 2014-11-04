using System.Data.Entity;
using System.Runtime.CompilerServices;

namespace EntityFramework.Triggers {
	internal static class TriggersWeakRefs<TDbContext> where TDbContext : DbContext {
		internal static readonly ConditionalWeakTable<ITriggerable, ITriggers<TDbContext>> ConditionalWeakTable = new ConditionalWeakTable<ITriggerable, ITriggers<TDbContext>>();
	}
}
