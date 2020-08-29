using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Json.Pointer
{
	/// <summary>
	/// More extensions on <see cref="IEnumerable{T}"/>.
	/// </summary>
	public static class EnumerableExtensions
	{
		/// <summary>
		/// Gets a collection-oriented hash code by combining the hash codes of its elements.
		/// </summary>
		/// <typeparam name="T">The type of element.</typeparam>
		/// <param name="collection">The collection of elements.</param>
		/// <returns>A singular integer value that represents the collection.</returns>
		/// <remarks>This can be used to correctly compare the contents of collections.</remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetCollectionHashCode<T>(this IEnumerable<T> collection)
		{
			return collection.Aggregate(0, (current, obj) => unchecked(current * 397) ^ (obj?.GetHashCode() ?? 0));
		}
	}
}