#if !NET9_0_OR_GREATER

using System;
using System.Collections.Generic;

namespace Json.Schema.Generation;

internal static class DotnetCompatibility
{
	public static IEnumerable<TItem> DistinctBy<TItem, TTarget>(this IEnumerable<TItem> items, Func<TItem, TTarget> selector)
	{
		var found = new HashSet<TTarget>();
		foreach (var item in items)
		{
			var target = selector(item);
			if (!found.Add(target)) continue;

			yield return item;
		}
	}
}

#endif