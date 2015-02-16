using System;
using System.ComponentModel.DataAnnotations;
using EntityFramework.Triggers;

namespace Tests {
	public class Person : EntityWithTracking, ITriggerable {
		[Key]
		public Int64 Id { get; protected set; }
		public String FirstName { get; set; }
		[Required]
		public String LastName { get; set; }
		public Boolean IsMarkedDeleted { get; set; }
	}
}
