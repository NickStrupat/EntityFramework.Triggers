using System;
using System.Collections.Generic;

namespace EntityFrameworkCore.Triggers;

public sealed class ArrayEqualityComparer<TElement> : IEqualityComparer<TElement[]>
{
	public static readonly ArrayEqualityComparer<TElement> Default = new(EqualityComparer<TElement>.Default);

	private readonly IEqualityComparer<TElement> elementEqualityComparer;

	public ArrayEqualityComparer(IEqualityComparer<TElement> elementEqualityComparer)
	{
        ArgumentNullException.ThrowIfNull(elementEqualityComparer);
        this.elementEqualityComparer = elementEqualityComparer;
	}

	public Boolean Equals(TElement[]? x, TElement[]? y)
	{
		if (ReferenceEquals(x, y)) // both null or both referencing the same array object
			return true;
		if (x is null || y is null)
			return false;
		if (x.Length != y.Length)
			return false;
		for (var i = 0; i != x.Length; i++)
			if (!elementEqualityComparer.Equals(x[i], y[i]))
				return false;
		return true;
	}

	public Int32 GetHashCode(TElement[] types)
	{
		ArgumentNullException.ThrowIfNull(types);
		return HashCode.Combine(types.Length);
	}
}