using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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
		public static bool ContentsEqual<T>(this IReadOnlyList<T>? collection, IReadOnlyList<T>? other)
		{
			if (collection == null && other == null) return true;
			if (collection == null || other == null) return false;

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
		public static bool ContentsEqual<T>(this IReadOnlyList<T>? collection, IReadOnlyList<T>? other, IEqualityComparer<T> comparer)
		{
			if (collection == null && other == null) return true;
			if (collection == null || other == null) return false;

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

		/// <summary>
		/// Gets a string-dictionary-oriented hash code by combining the hash codes of its elements.
		/// </summary>
		/// <typeparam name="T">The type of element.</typeparam>
		/// <param name="collection">The collection of elements.</param>
		/// <returns>A singular integer value that represents the collection.</returns>
		/// <remarks>
		/// This can be used to correctly compare the contents of string dictionaries where
		/// key ordering is not important.
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetStringDictionaryHashCode<T>(this IDictionary<string, T> collection)
		{
			static int GetHashCode(KeyValuePair<string, T> kvp)
			{
				unchecked
				{
					var hashCode = kvp.Key.GetHashCode();
					hashCode = (hashCode * 397) ^ kvp.Value?.GetHashCode() ?? 0;
					return hashCode;
				}
			}

			return collection.OrderBy(s => s.Key, StringComparer.InvariantCulture)
				.Aggregate(0, (current, obj) => unchecked(current * 397) ^ GetHashCode(obj));
		}

		/// <summary>
		/// Gets a string-dictionary-oriented hash code by combining the hash codes of its elements.
		/// </summary>
		/// <typeparam name="T">The type of element.</typeparam>
		/// <param name="collection">The collection of elements.</param>
		/// <returns>A singular integer value that represents the collection.</returns>
		/// <remarks>
		/// This can be used to correctly compare the contents of string dictionaries where
		/// key ordering is not important.
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetStringDictionaryHashCode<T>(this IReadOnlyDictionary<string, T> collection)
		{
			static int GetHashCode(KeyValuePair<string, T> kvp)
			{
				unchecked
				{
					var hashCode = kvp.Key.GetHashCode();
					hashCode = (hashCode * 397) ^ kvp.Value?.GetHashCode() ?? 0;
					return hashCode;
				}
			}

			return collection.OrderBy(s => s.Key, StringComparer.InvariantCulture)
				.Aggregate(0, (current, obj) => unchecked(current * 397) ^ GetHashCode(obj));
		}

		/// <summary>
		/// Gets a collection-oriented hash code by combining the hash codes of its elements.
		/// </summary>
		/// <typeparam name="T">The type of element.</typeparam>
		/// <param name="collection">The collection of elements.</param>
		/// <returns>A singular integer value that represents the collection.</returns>
		/// <remarks>This can be used to correctly compare the contents of collections.</remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetUnorderedCollectionHashCode<T>(this IEnumerable<T> collection)
		{
			int result = 0;
			unchecked
			{
				foreach (var item in collection)
				{
					result += item?.GetHashCode() ?? 0;
				}
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static int GetUnorderedCollectionHashCode<T>(this IEnumerable<T> collection, Func<T, int> getHashCode)
		{
			int result = 0;
			unchecked
			{
				foreach (var item in collection)
				{
					if (item == null) continue;

					result += getHashCode(item);
				}
			}
			return result;
		}
	}
}