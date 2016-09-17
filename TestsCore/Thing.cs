using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


#if EF_CORE
namespace EntityFrameworkCore.Triggers.Tests {
#else
namespace EntityFramework.Triggers.Tests {
#endif

	public class Thing {
        public virtual Int64 Id             { get; private set; }

        [Required]
		public virtual String Value         { get; set; }

		[NotMapped] public virtual Boolean Inserting    { get; set; }
		[NotMapped] public virtual Boolean InsertFailed { get; set; }
		[NotMapped] public virtual Boolean Inserted     { get; set; }
		[NotMapped] public virtual Boolean Updating     { get; set; }
		[NotMapped] public virtual Boolean UpdateFailed { get; set; }
		[NotMapped] public virtual Boolean Updated      { get; set; }
		[NotMapped] public virtual Boolean Deleting     { get; set; }
		[NotMapped] public virtual Boolean DeleteFailed { get; set; }
		[NotMapped] public virtual Boolean Deleted      { get; set; }
	}
}
