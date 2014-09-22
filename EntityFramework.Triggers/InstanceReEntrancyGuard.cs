using System;
using System.Runtime.CompilerServices;

namespace EntityFramework.Triggers {
	internal class InstanceReEntrancyGuard : IDisposable {
		private class BooleanRef { public Boolean Value { get; set; } }
		private static readonly ConditionalWeakTable<Object, BooleanRef> doItReEntranceEphemerons = new ConditionalWeakTable<Object, BooleanRef>();
		private readonly BooleanRef hasEnteredOnce;
		public InstanceReEntrancyGuard(Object instance, String reEntrantMessage = "", Func<Boolean> extraCheck = null) {
			hasEnteredOnce = doItReEntranceEphemerons.GetOrCreateValue(instance);
			if ((extraCheck != null && extraCheck()) || hasEnteredOnce.Value)
				throw new InvalidOperationException((hasEnteredOnce.Value ? "Re-entrance detected." : "") + (String.IsNullOrWhiteSpace(reEntrantMessage) ? "" : (" " + reEntrantMessage)));
			hasEnteredOnce.Value = true;
		}
		public void Dispose() {
			hasEnteredOnce.Value = false;
		}
	}
}
