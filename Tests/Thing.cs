using System;
using EntityFramework.Triggers;

namespace Tests {
    public class Thing {
        public Int64 Id { get; protected set; }
        public String Value { get; set; }
    }

	public class TriggerableThing : ITriggerable<TriggerableThing> {
		public Int64 Id { get; protected set; }
		public String Value { get; set; }
	}
}
