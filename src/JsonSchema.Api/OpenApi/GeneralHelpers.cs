using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Api.OpenApi;

internal static class GeneralHelpers
{
	public static IEnumerable<T> Collect<T>(params IEnumerable<T>?[] collections)
	{
		return collections.Where(x => x != null).SelectMany(x => x!);
	}

	public static object? Resolve(this IRefTargetContainer container, ReadOnlySpan<string> keys)
	{
		return container.Resolve(keys);
	}
}