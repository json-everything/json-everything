using System.Collections.Generic;
using System.Linq;

namespace Json.Schema
{
	internal static class EnumerableExtensions
	{
		public static bool ContentsEqual<T>(this IReadOnlyList<T> collection, IReadOnlyList<T> other)
		{
			if (collection.Count != other.Count) return false;

			var grouped = collection.GroupBy(x => x);
			if (grouped.Any(g => g.Count() != other.Count(x => Equals(x, g.First())))) return false;

			return !other.Except(collection).Any();
		}

		public static bool ContentsEqual<T>(this IReadOnlyList<T> collection, IReadOnlyList<T> other, IEqualityComparer<T> comparer)
		{
			if (collection.Count != other.Count) return false;

			var grouped = collection.GroupBy(x => x);
			if (grouped.Any(g => g.Count() != other.Count(x => comparer.Equals(x, g.First())))) return false;

			return !other.Except(collection).Any();
		}
	}
}