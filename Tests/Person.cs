using System;
using System.ComponentModel.DataAnnotations;
using EntityFramework.Triggers;

namespace Tests {
    public class Person : EntityWithTriggers<Person, Context> {
        [Key]
        public Int64 Id { get; protected set; }
        public String FirstName { get; set; }
		[Required]
		public String LastName { get; set; }
		public Boolean IsMarkedDeleted { get; set; }
    }

	public class Person2 : ITriggerable {
		public Triggers<Person2, Context2> Triggers { get { return this.Triggers<Person2, Context2>(); } }

		[Key]
		public Int64 Id { get; protected set; }
		public String FirstName { get; set; }
		[Required]
		public String LastName { get; set; }
		public Boolean IsMarkedDeleted { get; set; }
	}
}
