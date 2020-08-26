using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Json.Pointer
{
	public static class EnumerableExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetCollectionHashCode<T>(this IEnumerable<T> collection)
		{
			return collection.Aggregate(0, (current, obj) => unchecked(current * 397) ^ (obj?.GetHashCode() ?? 0));
		}
	}
}