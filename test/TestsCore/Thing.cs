using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


#if EF_CORE
namespace EntityFrameworkCore.Triggers.Tests {
#else
namespace EntityFramework.Triggers.Tests {
#endif
	public interface IThing {
		Int64 Id             { get; }
		String Value         { get; set; }
		Boolean Inserting    { get; set; }
		Boolean InsertFailed { get; set; }
		Boolean Inserted     { get; set; }
		Boolean Updating     { get; set; }
		Boolean UpdateFailed { get; set; }
		Boolean Updated      { get; set; }
		Boolean Deleting     { get; set; }
		Boolean DeleteFailed { get; set; }
		Boolean Deleted      { get; set; }
		List<Int32> Numbers  { get; }
	}

	public class Thing : IThing {
		[Key]
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

		[NotMapped] public virtual List<Int32> Numbers { get; } = new List<Int32>();
	}

	public interface IApple {
		List<Int32> Numbers { get; }
	}

	public class Apple : Thing, IApple { }

	public interface IRoyalGala {
		List<Int32> Numbers { get; }
	}

	public class RoyalGala : Apple, IRoyalGala { }
}
