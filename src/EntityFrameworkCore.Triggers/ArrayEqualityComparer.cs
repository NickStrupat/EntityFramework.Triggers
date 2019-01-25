using System;
using System.Collections.Generic;

#if EF_CORE
namespace EntityFrameworkCore.Triggers
#else
namespace EntityFramework.Triggers
#endif
{
	public class ArrayEqualityComparer<TElement> : IEqualityComparer<TElement[]>
	{
		public static readonly ArrayEqualityComparer<TElement> Default =
			new ArrayEqualityComparer<TElement>(EqualityComparer<TElement>.Default);

		private readonly IEqualityComparer<TElement> elementEqualityComparer;

		public ArrayEqualityComparer(IEqualityComparer<TElement> elementEqualityComparer) =>
			this.elementEqualityComparer = elementEqualityComparer ?? throw new ArgumentNullException(nameof(elementEqualityComparer));

		public Boolean Equals(TElement[] x, TElement[] y)
		{
			if (ReferenceEquals(x, y)) // both null or both referencing the same array object
				return true;
			if (x is null | y is null)
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
			var hashCode = 0x51ed270b;
			if (types == null)
				return hashCode;
			for (var i = 0; i != types.Length; i++)
				hashCode = hashCode * -1521134295 + elementEqualityComparer.GetHashCode(types[i]);
			return hashCode;
		}
	}
}
