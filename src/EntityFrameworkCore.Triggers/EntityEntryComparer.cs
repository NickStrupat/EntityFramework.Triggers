using System;
using System.Collections.Generic;

#if EF_CORE
using Microsoft.EntityFrameworkCore.ChangeTracking;
namespace EntityFrameworkCore.Triggers
#else
using EntityEntry = System.Data.Entity.Infrastructure.DbEntityEntry;
namespace EntityFramework.Triggers
#endif
{
	internal class EntityEntryComparer : IEqualityComparer<EntityEntry>
	{
		private EntityEntryComparer() { }
		public Boolean Equals(EntityEntry x, EntityEntry y) => ReferenceEquals(x?.Entity, y?.Entity);
		public Int32 GetHashCode(EntityEntry obj) => obj.Entity.GetHashCode();
		public static readonly EntityEntryComparer Default = new EntityEntryComparer();
	}
}