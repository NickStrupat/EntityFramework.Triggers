using System;
using System.Collections.Generic;
using System.Linq;
#if EF_CORE
namespace EntityFrameworkCore.Triggers
#else
namespace EntityFramework.Triggers
#endif
{
	public sealed class TriggersEqualityComparer<TTriggers> : IEqualityComparer<TTriggers>
	where TTriggers : class, ITriggers
	{
		public static readonly TriggersEqualityComparer<TTriggers> Instance = new TriggersEqualityComparer<TTriggers>();

		public Boolean Equals(TTriggers x, TTriggers y)
		{
			if (ReferenceEquals(x, y))
				return true;
			if (x is null || y is null)
				return false;

			foreach (var tes in GetTriggerEvents(x).Zip(GetTriggerEvents(y), (tex, tey) => (tex, tey)))
				if (!ReferenceEquals(tes.tex, tes.tey))
					return false;
			return true;
		}

		public Int32 GetHashCode(TTriggers triggers)
		{
			var hashCode = 0x51ed270b;
			foreach (var triggerEvent in GetTriggerEvents(triggers))
				if (triggerEvent != null)
					hashCode = (hashCode * -1521134295) + triggerEvent.GetHashCode();
			return hashCode;
		}

		private static IEnumerable<TriggerEvent> GetTriggerEvents(TTriggers triggers) => EventGetters.Select(x => x.Invoke(triggers));

		private static readonly EventDelegate[] EventGetters = GetEventGetterQuery.ToArray();

		private static IEnumerable<EventDelegate> GetEventGetterQuery =>
			from propertyInfo in typeof(TTriggers).GetProperties()
			where typeof(TriggerEvent).IsAssignableFrom(propertyInfo.PropertyType)
			orderby propertyInfo.Name
			select propertyInfo.GetGetMethod().CreateDelegate<EventDelegate>();

		private delegate TriggerEvent EventDelegate(TTriggers triggers);
	}
}