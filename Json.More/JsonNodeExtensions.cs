using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Json.More;

/// <summary>
/// Provides extension functionality for <see cref="JsonNode"/>.
/// </summary>
public static class JsonNodeExtensions
{
	/// <summary>
	/// Determines JSON-compatible equivalence.
	/// </summary>
	/// <param name="a">The first element.</param>
	/// <param name="b">The second element.</param>
	/// <returns><code>true</code> if the element are equivalent; <code>false</code> otherwise.</returns>
	public static bool IsEquivalentTo(this JsonNode? a, JsonNode? b)
	{
		switch (a, b)
		{
			case (null, null):
				return true;
			case (JsonObject objA, JsonObject objB):
				if (objA.Count != objB.Count) return false;
				var grouped = objA.Concat(objB)
					.GroupBy(p => p.Key)
					.Select(g => g.ToList())
					.ToList();
				return grouped.All(g => g.Count == 2 && g[0].Value.IsEquivalentTo(g[1].Value));
			case (JsonArray arrayA, JsonArray arrayB):
				if (arrayA.Count != arrayB.Count) return false;
				var zipped = arrayA.Zip(arrayB, (ae, be) => (ae, be));
				return zipped.All(p => p.ae.IsEquivalentTo(p.be));
			case (JsonValue aValue, JsonValue bValue):
				if (aValue.GetValue<object>() is JsonElement aElement &&
				    bValue.GetValue<object>() is JsonElement bElement)
					return aElement.IsEquivalentTo(bElement);
				return a.ToJsonString() == b.ToJsonString();
			default:
				return a?.ToJsonString() == b?.ToJsonString();
		}
	}

	// source: https://stackoverflow.com/a/60592310/878701, modified for netstandard2.0
	// license: https://creativecommons.org/licenses/by-sa/4.0/
	/// <summary>
	/// Generate a consistent JSON-value-based hash code for the element.
	/// </summary>
	/// <param name="node">The element.</param>
	/// <param name="maxHashDepth">Maximum depth to calculate.  Default is -1 which utilizes the entire structure without limitation.</param>
	/// <returns>The hash code.</returns>
	/// <remarks>
	/// See the following for discussion on why the default implementation is insufficient:
	///
	/// - https://github.com/gregsdennis/json-everything/issues/76
	/// - https://github.com/dotnet/runtime/issues/33388
	/// </remarks>
	public static int GetEquivalenceHashCode(this JsonNode node, int maxHashDepth = -1)
	{
		static void Add(ref int current, object? newValue)
		{
			unchecked
			{
				current = current * 397 ^ (newValue?.GetHashCode() ?? 0);
			}
		}

		// ReSharper disable once InconsistentNaming
		void ComputeHashCode(JsonNode? target, ref int current, int depth)
		{
			if (target == null) return;

			Add(ref current, target.GetType());

			switch (target)
			{
				case JsonArray array:
					if (depth != maxHashDepth)
						foreach (var item in array)
							ComputeHashCode(item, ref current, depth + 1);
					else
						Add(ref current, array.Count);
					break;

				case JsonObject obj:
					foreach (var property in obj.OrderBy(p => p.Key, StringComparer.Ordinal))
					{
						Add(ref current, property.Key);
						if (depth != maxHashDepth)
							ComputeHashCode(property.Value, ref current, depth + 1);
					}
					break;
				default:
					var value = target.AsValue();
					if (value.TryGetValue<bool>(out var boolA))
						Add(ref current, boolA);
					else
					{
						var number = value.GetNumber();
						if (number != null)
							Add(ref current, number);
						else if (value.TryGetValue<string>(out var stringA))
							Add(ref current, stringA);
					}

					break;
			}
		}

		var hash = 0;
		ComputeHashCode(node, ref hash, 0);
		return hash;
	}

	public static string AsJsonString(this JsonNode? node)
	{
		return node?.ToJsonString() ?? "null";
	}

	public static decimal? GetNumber(this JsonValue value)
	{
		var number = GetInteger(value);
		if (number != null) return number;

		if (value.TryGetValue<float>(out var f)) return (decimal)f;
		if (value.TryGetValue<double>(out var d)) return (decimal)d;
		if (value.TryGetValue<decimal>(out var dc)) return dc;

		return null;
	}

	public static long? GetInteger(this JsonValue value)
	{
		if (value.TryGetValue<byte>(out var b)) return b;
		if (value.TryGetValue<short>(out var s)) return s;
		if (value.TryGetValue<ushort>(out var us)) return us;
		if (value.TryGetValue<int>(out var i)) return i;
		if (value.TryGetValue<ushort>(out var ui)) return ui;
		if (value.TryGetValue<long>(out var l)) return l;
		// this doesn't feel right... throw?
		if (value.TryGetValue<ulong>(out var ul)) return (long)ul;

		return null;
	}

	public static JsonNode? Copy(this JsonNode? source)
	{
		return source.Deserialize<JsonNode?>();
	}

	public static bool TryGetValue(this JsonObject obj, string propertyName, out JsonNode? node, out Exception? e)
	{
		e = null;
		try
		{
			return obj.TryGetPropertyValue(propertyName, out node);
		}
		catch (ArgumentException ae)
		{
			e = ae;
			node = null;
			return false;
		}
	}
}