using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityFrameworkTriggers;

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
