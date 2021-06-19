using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace Json.More
{
	/// <summary>
	/// Provides extension functionality for <see cref="JsonElement"/>.
	/// </summary>
	public static class JsonElementExtensions
	{
		/// <summary>
		/// Determines JSON-compatible equivalence.
		/// </summary>
		/// <param name="a">The first element.</param>
		/// <param name="b">The second element.</param>
		/// <returns><code>true</code> if the element are equivalent; <code>false</code> otherwise.</returns>
		/// <exception cref="ArgumentOutOfRangeException">The <see cref="JsonElement.ValueKind"/> is not valid.</exception>
		public static bool IsEquivalentTo(this JsonElement a, JsonElement b)
		{
			if (a.ValueKind != b.ValueKind) return false;
			switch (a.ValueKind)
			{
				case JsonValueKind.Object:
					var aProperties = a.EnumerateObject().ToList();
					var bProperties = b.EnumerateObject().ToList();
					if (aProperties.Count != bProperties.Count) return false;
					var grouped = aProperties.Concat(bProperties)
						.GroupBy(p => p.Name)
						.Select(g => g.ToList())
						.ToList();
					return grouped.All(g => g.Count == 2 && g[0].Value.IsEquivalentTo(g[1].Value));
				case JsonValueKind.Array:
					var aElements = a.EnumerateArray().ToList();
					var bElements = b.EnumerateArray().ToList();
					if (aElements.Count != bElements.Count) return false;
					var zipped = aElements.Zip(bElements, (ae, be) => (ae, be));
					return zipped.All(p => p.ae.IsEquivalentTo(p.be));
				case JsonValueKind.String:
					return a.GetString() == b.GetString();
				case JsonValueKind.Number:
					return a.GetDecimal() == b.GetDecimal();
				case JsonValueKind.Undefined:
					return false;
				case JsonValueKind.True:
				case JsonValueKind.False:
				case JsonValueKind.Null:
					return true;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		// source: https://stackoverflow.com/a/60592310/878701, modified for netstandard2.0
		// license: https://creativecommons.org/licenses/by-sa/4.0/
		/// <summary>
		/// Generate a consistent JSON-value-based hash code for the element.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="maxHashDepth">Maximum depth to calculate.  Default is -1 which utilizes the entire structure without limitation.</param>
		/// <returns>The hash code.</returns>
		/// <remarks>
		/// See the following for discussion on why the default implementation is insufficient:
		///
		/// - https://github.com/gregsdennis/json-everything/issues/76
		/// - https://github.com/dotnet/runtime/issues/33388
		/// </remarks>
		public static int GetEquivalenceHashCode(this JsonElement element, int maxHashDepth = -1)
		{
			static void Add(ref int current, object? newValue)
			{
				unchecked
				{
					current = current * 397 ^ (newValue?.GetHashCode() ?? 0);
				}
			}

			void ComputeHashCode(JsonElement obj, ref int current, int depth)
			{
				Add(ref current, obj.ValueKind);

				switch (obj.ValueKind)
				{
					case JsonValueKind.Null:
					case JsonValueKind.True:
					case JsonValueKind.False:
					case JsonValueKind.Undefined:
						break;

					case JsonValueKind.Number:
						Add(ref current, obj.GetRawText());
						break;

					case JsonValueKind.String:
						Add(ref current, obj.GetString());
						break;

					case JsonValueKind.Array:
						if (depth != maxHashDepth)
							foreach (var item in obj.EnumerateArray())
								ComputeHashCode(item, ref current, depth + 1);
						else
							Add(ref current, obj.GetArrayLength());
						break;

					case JsonValueKind.Object:
						foreach (var property in obj.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal))
						{
							Add(ref current, property.Name);
							if (depth != maxHashDepth)
								ComputeHashCode(property.Value, ref current, depth + 1);
						}
						break;

					default:
						throw new JsonException($"Unknown JsonValueKind {obj.ValueKind}");
				}
			}

			var hash = 0;
			ComputeHashCode(element, ref hash, 0);
			return hash;

		}

		/// <summary>
		/// Just a shortcut for calling `JsonSerializer.Serialize()` because `.ToString()` doesn't do what you might expect.
		/// </summary>
		/// <param name="element">The value to convert.</param>
		/// <returns>A JSON string.</returns>
		/// <remarks>
		/// See https://github.com/dotnet/runtime/issues/42502
		/// </remarks>
		public static string ToJsonString(this JsonElement element)
		{
			return JsonSerializer.Serialize(element);
		}

		/// <summary>
		/// Converts a <see cref="long"/> to a <see cref="JsonElement"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
		/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
		public static JsonElement AsJsonElement(this long value)
		{
			using var doc = JsonDocument.Parse(value.ToString(CultureInfo.InvariantCulture));
			return doc.RootElement.Clone();
		}

		/// <summary>
		/// Converts a <see cref="int"/> to a <see cref="JsonElement"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
		/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
		public static JsonElement AsJsonElement(this int value)
		{
			using var doc = JsonDocument.Parse(value.ToString(CultureInfo.InvariantCulture));
			return doc.RootElement.Clone();
		}

		/// <summary>
		/// Converts a <see cref="short"/> to a <see cref="JsonElement"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
		/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
		public static JsonElement AsJsonElement(this short value)
		{
			using var doc = JsonDocument.Parse(value.ToString(CultureInfo.InvariantCulture));
			return doc.RootElement.Clone();
		}

		/// <summary>
		/// Converts a <see cref="bool"/> to a <see cref="JsonElement"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
		/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
		public static JsonElement AsJsonElement(this bool value)
		{
			using var doc = JsonDocument.Parse(value ? "true" : "false");
			return doc.RootElement.Clone();
		}

		/// <summary>
		/// Converts a <see cref="long"/> to a <see cref="JsonElement"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
		/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
		public static JsonElement AsJsonElement(this decimal value)
		{
			using var doc = JsonDocument.Parse(value.ToString(CultureInfo.InvariantCulture));
			return doc.RootElement.Clone();
		}

		/// <summary>
		/// Converts a <see cref="double"/> to a <see cref="JsonElement"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
		/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
		public static JsonElement AsJsonElement(this double value)
		{
			using var doc = JsonDocument.Parse(value.ToString(CultureInfo.InvariantCulture));
			return doc.RootElement.Clone();
		}

		/// <summary>
		/// Converts a <see cref="float"/> to a <see cref="JsonElement"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
		/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
		public static JsonElement AsJsonElement(this float value)
		{
			using var doc = JsonDocument.Parse(value.ToString(CultureInfo.InvariantCulture));
			return doc.RootElement.Clone();
		}

		/// <summary>
		/// Converts a <see cref="string"/> to a <see cref="JsonElement"/>.  Can also be used to get a `null` element.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
		/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
		public static JsonElement AsJsonElement(this string? value)
		{
			using var doc = JsonDocument.Parse(JsonSerializer.Serialize(value));
			return doc.RootElement.Clone();
		}

		/// <summary>
		/// Converts a <see cref="long"/> to a <see cref="JsonElement"/>.
		/// </summary>
		/// <param name="values">The array of values to convert.</param>
		/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
		/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
		public static JsonElement AsJsonElement(this IEnumerable<JsonElement> values)
		{
			using var doc = JsonDocument.Parse($"[{string.Join(",", values.Select(v => v.ToJsonString()))}]");
			return doc.RootElement.Clone();
		}

		/// <summary>
		/// Converts a <see cref="long"/> to a <see cref="JsonElement"/>.
		/// </summary>
		/// <param name="values">The value to convert.</param>
		/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
		/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
		public static JsonElement AsJsonElement(this IDictionary<string, JsonElement> values)
		{
			using var doc = JsonDocument.Parse($"{{{string.Join(",", values.Select(v => $"{JsonSerializer.Serialize(v.Key)}:{v.Value.ToJsonString()}"))}}}");
			return doc.RootElement.Clone();
		}
	}
}