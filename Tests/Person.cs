using System;
using System.ComponentModel.DataAnnotations;
using EntityFrameworkTriggers;

namespace Tests {
    public class Person : EntityWithTriggers<Person, Context> {
        [Key]
        public Int64 Id { get; protected set; }
        public String FirstName { get; set; }
		[Required]
        public String LastName { get; set; }
    }

	public class TriggerablePerson : ITriggerable<TriggerablePerson> {
		[Key]
		public Int64 Id { get; protected set; }
		public String FirstName { get; set; }
		[Required]
		public String LastName { get; set; }
	}
}
