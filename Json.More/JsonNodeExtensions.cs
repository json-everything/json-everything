using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Json.More;

/// <summary>
/// Provides extension functionality for <see cref="JsonNode"/>.
/// </summary>
public static class JsonNodeExtensions
{
	private static readonly JsonSerializerOptions _unfriendlyCharSerialization = new()
	{
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};

#if NET8_0
	/// <summary>
	/// Determines JSON-compatible equivalence.
	/// </summary>
	/// <param name="a">The first element.</param>
	/// <param name="b">The second element.</param>
	/// <returns>`true` if the element are equivalent; `false` otherwise.</returns>
	/// <remarks>
	/// <see cref="JsonNode.DeepEquals(JsonNode,JsonNode)"/> has trouble testing numeric
	/// equality when `decimal` is involved.  As such it is still advised to use this
	/// method instead.  See https://github.com/dotnet/runtime/issues/97490.
	/// </remarks>
#else
	/// <summary>
	/// Determines JSON-compatible equivalence.
	/// </summary>
	/// <param name="a">The first element.</param>
	/// <param name="b">The second element.</param>
	/// <returns>`true` if the element are equivalent; `false` otherwise.</returns>
#endif
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

				var aNumber = aValue.GetNumber();
				var bNumber = bValue.GetNumber();
				if (aNumber != null) return aNumber == bNumber;

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

	/// <summary>
	/// Gets JSON string representation for <see cref="JsonNode"/>, including null support.
	/// </summary>
	/// <param name="node">A node.</param>
	/// <param name="options">Serializer options</param>
	/// <returns>JSON string representation.</returns>
	public static string AsJsonString(this JsonNode? node, JsonSerializerOptions? options = null)
	{
		return node?.ToJsonString(options) ?? "null";
	}

	/// <summary>
	/// Gets a node's underlying numeric value.
	/// </summary>
	/// <param name="value">A JSON value.</param>
	/// <returns>Gets the underlying numeric value, or null if the node represented a non-numeric value.</returns>
	public static decimal? GetNumber(this JsonValue value)
	{
		if (value.TryGetValue(out JsonElement e))
		{
			if (e.ValueKind != JsonValueKind.Number) return null;
			return e.GetDecimal();
		}

		var number = GetInteger(value);
		if (number != null) return number;

		if (value.TryGetValue(out float f)) return (decimal)f;
		if (value.TryGetValue(out double d)) return (decimal)d;
		if (value.TryGetValue(out decimal dc)) return dc;

		return null;
	}

	/// <summary>
	/// Gets a node's underlying numeric value if it's an integer.
	/// </summary>
	/// <param name="value">A JSON value.</param>
	/// <returns>Gets the underlying numeric value if it's an integer, or null if the node represented a non-integer value.</returns>
	public static long? GetInteger(this JsonValue value)
	{
		if (value.TryGetValue(out JsonElement e))
		{
			if (e.ValueKind != JsonValueKind.Number) return null;
			var d = e.GetDecimal();
			if (d == Math.Floor(d)) return (long)d;
			return null;
		}
		if (value.TryGetValue(out byte b)) return b;
		if (value.TryGetValue(out sbyte sb)) return sb;
		if (value.TryGetValue(out short s)) return s;
		if (value.TryGetValue(out ushort us)) return us;
		if (value.TryGetValue(out int i)) return i;
		if (value.TryGetValue(out ushort ui)) return ui;
		if (value.TryGetValue(out long l)) return l;
		// this doesn't feel right... throw?
		if (value.TryGetValue(out ulong ul)) return (long)ul;

		return null;
	}

#if !NET8_0_OR_GREATER
	/// <summary>
	/// Creates a deep copy of a node.
	/// </summary>
	/// <param name="source">A node.</param>
	/// <returns>A duplicate of the node.</returns>
	/// <remarks>
	///	`JsonNode` may only be part of a single JSON tree, i.e. have a single parent.
	/// Copying a node allows its value to be saved to another JSON tree.
	/// </remarks>
	public static JsonNode? Copy(this JsonNode? source)
	{
		JsonNode CopyObject(JsonObject obj)
		{
			var newObj = new JsonObject(obj.Options);
			foreach (var kvp in obj)
			{
				newObj[kvp.Key] = kvp.Value.Copy();
			}

			return newObj;
		}

		JsonNode CopyArray(JsonArray arr)
		{
			var newArr = new JsonArray(arr.Options);
			foreach (var item in arr)
			{
				newArr.Add(item.Copy());
			}

			return newArr;
		}

		JsonNode? CopyValue(JsonValue val)
		{
			return JsonValue.Create(val.GetValue<object>());
		}

		return source switch
		{
			null => null,
			JsonObject obj => CopyObject(obj),
			JsonArray arr => CopyArray(arr),
			JsonValue val => CopyValue(val),
			_ => throw new ArgumentOutOfRangeException(nameof(source))
		};
	}
#endif

	/// <summary>
	/// Convenience method that wraps <see cref="JsonObject.TryGetPropertyValue(string, out JsonNode?)"/>
	/// and catches argument exceptions.
	/// </summary>
	/// <param name="obj">The JSON object.</param>
	/// <param name="propertyName">The property name</param>
	/// <param name="node">The node under the property name if it exists and is singular; null otherwise.</param>
	/// <param name="e">An exception if one was thrown during the access attempt.</param>
	/// <returns>true if the property exists and is singular within the JSON data.</returns>
	/// <remarks>
	/// <see cref="JsonObject.TryGetPropertyValue(string, out JsonNode?)"/> throws an
	/// <see cref="ArgumentException"/> if the node was parsed from data that has duplicate
	/// keys.  Please see https://github.com/dotnet/runtime/issues/70604 for more information.
	/// </remarks>
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

	/// <summary>
	/// Creates a new <see cref="JsonArray"/> by copying from an enumerable of nodes.
	/// </summary>
	/// <param name="nodes">The nodes.</param>
	/// <returns>A JSON array.</returns>
	/// <remarks>
	///	`JsonNode` may only be part of a single JSON tree, i.e. have a single parent.
	/// Copying a node allows its value to be saved to another JSON tree.
	/// </remarks>
	public static JsonArray ToJsonArray(this IEnumerable<JsonNode?> nodes)
	{
#if NET8_0_OR_GREATER
		return new JsonArray(nodes.Select(x => x?.DeepClone()).ToArray());
#else
		return new JsonArray(nodes.Select(x => x.Copy()).ToArray());
#endif
	}

	///  <summary>
	///  Gets a JSON Path string that indicates the node's location within
	///  its JSON structure.
	///  </summary>
	///  <param name="node">The node to find.</param>
	///  <param name="useShorthand">Determines whether shorthand syntax is used when possible, e.g. `$.foo`.</param>
	///  <exception cref="ArgumentNullException">Null nodes cannot be located as the parent cannot be determined.</exception>
	///  <returns>
	/// 	A string containing a JSON Path.
	///  </returns>
	public static string GetPathFromRoot(this JsonNode node, bool useShorthand = false)
	{
		var current = node ?? throw new ArgumentNullException(nameof(node), "null nodes cannot be located");

		var segments = GetSegments(current);

		var sb = new StringBuilder();
		sb.Append('$');
		segments.Pop();  // first is always null - the root
		while (segments.Any())
		{
			var segment = segments.Pop();
			var index = segment?.GetNumber();
			sb.Append(index != null ? $"[{index}]" : GetNamedSegmentForPath(segment!, useShorthand));
		}

		return sb.ToString();
	}

	private static Stack<JsonValue?> GetSegments(JsonNode? current)
	{
		var segments = new Stack<JsonValue?>();
		while (current != null)
		{
			var segment = current.Parent switch
			{
				null => null,
				JsonObject obj => GetKey(obj, current),
				JsonArray arr => GetIndex(arr, current),
				_ => throw new ArgumentOutOfRangeException("parent", "this shouldn't happen")
			};
			segments.Push(segment);
			current = current.Parent;
		}

		return segments;
	}

	private static JsonValue GetKey(JsonObject obj, JsonNode current)
	{
		return JsonValue.Create(obj.First(x => ReferenceEquals(x.Value, current)).Key)!;
	}

	private static JsonValue GetIndex(JsonArray arr, JsonNode current)
	{
		return JsonValue.Create(arr.IndexOf(current));
	}

	private static string GetNamedSegmentForPath(JsonValue segment, bool useShorthand)
	{
		var value = segment.GetValue<string>();
		if (useShorthand && Regex.IsMatch(value, "^[a-z][a-z_]*$"))  return $".{value}";

		return $"['{PrepForJsonPath(segment.AsJsonString(_unfriendlyCharSerialization))}']";
	}

	// pass JSON string because it will handle char escaping inside the string.
	// just need to replace the quotes.
	private static string PrepForJsonPath(string jsonString)
	{
		var content = jsonString.Substring(1, jsonString.Length-2);
		var escaped = content.Replace("\\\"", "\"")
			.Replace("'", "\\'");
		return escaped;
	}

	///  <summary>
	///  Gets a JSON Pointer string that indicates the node's location within
	///  its JSON structure.
	///  </summary>
	///  <param name="node">The node to find.</param>
	///  <exception cref="ArgumentNullException">Null nodes cannot be located as the parent cannot be determined.</exception>
	///  <returns>
	/// 	A string containing a JSON Pointer.
	///  </returns>
	public static string GetPointerFromRoot(this JsonNode node)
	{
		var current = node ?? throw new ArgumentNullException(nameof(node), "null nodes cannot be located");

		var segments = GetSegments(current);

		var sb = new StringBuilder();
		segments.Pop();  // first is always null - the root
		while (segments.Any())
		{
			var segment = segments.Pop();
			var index = segment?.GetNumber();
			sb.Append(index != null ? $"/{index}" : $"/{PrepForJsonPointer(segment!.GetValue<string>())}");
		}

		return sb.ToString();
	}

	private static string PrepForJsonPointer(string s)
	{
		var escaped = s.Replace("~", "~0")
			.Replace("/", "~1");
		return escaped;
	}
}