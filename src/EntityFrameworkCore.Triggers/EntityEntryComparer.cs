using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EntityFrameworkCore.Triggers;

internal class EntityEntryComparer : IEqualityComparer<EntityEntry>
{
	public static readonly EntityEntryComparer Default = new();

	private EntityEntryComparer() { }
	
	public Boolean Equals(EntityEntry? x, EntityEntry? y)
	{
		if (ReferenceEquals(x, y))
			return true;
		if (x is null || y is null)
			return false;
		return ReferenceEquals(x.Entity, y.Entity);
	}

	public Int32 GetHashCode(EntityEntry? obj) => obj?.Entity.GetHashCode() ?? 0;
}