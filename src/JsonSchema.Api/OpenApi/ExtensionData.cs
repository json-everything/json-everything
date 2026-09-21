using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Supports extension data for all types.
/// </summary>
public class ExtensionData : Dictionary<string, JsonNode?>, IRefTargetContainer
{
	object? IRefTargetContainer.Resolve(ReadOnlySpan<string> keys)
	{
		if (keys.Length == 0)
			throw new InvalidOperationException("Greg forgot to check for an empty span.");

		if (!TryGetValue(keys[0], out var jn)) return null;
		if (keys.Length == 1) return jn;

		keys[1..].ToPointer().TryEvaluate(jn, out var result);

		return result;
	}
}
