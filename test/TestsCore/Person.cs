using System;
using System.ComponentModel.DataAnnotations;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers.Tests {
#else
using System.Data.Entity;
namespace EntityFramework.Triggers.Tests {
#endif
	
	public class Person : EntityWithTracking {
		[Key]
		public virtual Int64 Id { get; private set; }
		public virtual String FirstName { get; set; }
		[Required]
		public virtual String LastName { get; set; }
		public virtual Boolean IsMarkedDeleted { get; set; }
	}
}
