using System.Collections.Generic;
using System.Linq;

namespace Json.Schema
{
	/// <summary>
	/// More extensions on <see cref="IEnumerable{T}"/>.
	/// </summary>
	public static class EnumerableExtensions
	{
		/// <summary>
		/// Determines contents-based equality: each element appears equally in each set.
		/// </summary>
		/// <param name="collection">The first collection.</param>
		/// <param name="other">The second collection.</param>
		/// <typeparam name="T">The element type.</typeparam>
		/// <returns><code>true</code> if the collections contain the same number of the same elements; <code>false</code> otherwise.</returns>
		public static bool ContentsEqual<T>(this IReadOnlyList<T> collection, IReadOnlyList<T> other)
		{
			if (collection == null && other == null) return true;
			if (collection == null) return false;

			if (collection.Count != other.Count) return false;

			var grouped = collection.GroupBy(x => x);
			var otherCopy = other.ToList();
			if (grouped.Any(g =>
				{
					var value = g.First();
					return g.Count() != otherCopy.RemoveAll(x => Equals(x, value));
				}))
				return false;

			return !otherCopy.Any();
		}

		/// <summary>
		/// Determines contents-based equality: each element appears equally in each set.
		/// </summary>
		/// <param name="collection">The first collection.</param>
		/// <param name="other">The second collection.</param>
		/// <param name="comparer">A custom equality comparer.</param>
		/// <typeparam name="T">The element type.</typeparam>
		/// <returns><code>true</code> if the collections contain the same number of the same elements; <code>false</code> otherwise.</returns>
		public static bool ContentsEqual<T>(this IReadOnlyList<T> collection, IReadOnlyList<T> other, IEqualityComparer<T> comparer)
		{
			if (collection == null && other == null) return true;
			if (collection == null) return false;

			if (collection.Count != other.Count) return false;

			var grouped = collection.GroupBy(x => x);
			var otherCopy = other.ToList();
			if (grouped.Any(g =>
				{
					var value = g.First();
					return g.Count() != otherCopy.RemoveAll(x => comparer.Equals(x, value));
				}))
				return false;

			return !otherCopy.Any();
		}
	}
}