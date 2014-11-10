using System;
using System.Runtime.CompilerServices;

namespace EntityFramework.Triggers {
	internal class InstanceReEntrancyGuard : IDisposable {
		private class Ref<T> where T : struct { public T Value { get; set; } }
		private static readonly ConditionalWeakTable<Object, Ref<Boolean>> reEntranceEphemerons = new ConditionalWeakTable<Object, Ref<Boolean>>();
		private readonly Ref<Boolean> hasEnteredOnce;
		public InstanceReEntrancyGuard(Object instance, String reEntrantMessage = "", Func<Boolean> extraCheck = null) {
			hasEnteredOnce = reEntranceEphemerons.GetValue(instance, x => new Ref<Boolean>());
			if ((extraCheck != null && extraCheck()) || hasEnteredOnce.Value)
				throw new InvalidOperationException((hasEnteredOnce.Value ? "Re-entrance detected." : "") + (String.IsNullOrWhiteSpace(reEntrantMessage) ? "" : (" " + reEntrantMessage)));
			hasEnteredOnce.Value = true;
		}
		public void Dispose() {
			hasEnteredOnce.Value = false;
		}
	}
}
