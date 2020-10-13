using System;
using System.Collections.Generic;
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

		/// <summary>
		/// Serializes a <see cref="JsonElement"/> into a proper JSON string.
		/// </summary>
		/// <param name="element">The value to convert.</param>
		/// <returns>A JSON string.</returns>
		/// <remarks>
		/// Booleans don't case right.  See https://github.com/dotnet/runtime/issues/42502
		/// </remarks>
		public static string ToJsonString(this JsonElement element)
		{
			return element.ValueKind switch
			{
				JsonValueKind.Object => $"{{{string.Join(",", element.EnumerateObject().Select(p => $"\"{p.Name}\":{p.Value.ToJsonString()}"))}}}",
				JsonValueKind.Array => $"[{string.Join(",", element.EnumerateArray().Select(i => i.ToJsonString()))}]",
				JsonValueKind.String => $"\"{element}\"",
				JsonValueKind.Number => element.ToString(),
				JsonValueKind.True => "true",
				JsonValueKind.False => "false",
				JsonValueKind.Null => "null",
				_ => throw new ArgumentOutOfRangeException()
			};
		}

		/// <summary>
		/// Converts a <see cref="long"/> to a <see cref="JsonElement"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
		/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
		public static JsonElement AsJsonElement(this long value)
		{
			var doc = JsonDocument.Parse($"{value}");
			return doc.RootElement;
		}

		/// <summary>
		/// Converts a <see cref="int"/> to a <see cref="JsonElement"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
		/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
		public static JsonElement AsJsonElement(this int value)
		{
			var doc = JsonDocument.Parse($"{value}");
			return doc.RootElement;
		}

		/// <summary>
		/// Converts a <see cref="short"/> to a <see cref="JsonElement"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
		/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
		public static JsonElement AsJsonElement(this short value)
		{
			var doc = JsonDocument.Parse($"{value}");
			return doc.RootElement;
		}

		/// <summary>
		/// Converts a <see cref="bool"/> to a <see cref="JsonElement"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
		/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
		public static JsonElement AsJsonElement(this bool value)
		{
			var doc = JsonDocument.Parse($"{value.ToString().ToLower()}");
			return doc.RootElement;
		}

		/// <summary>
		/// Converts a <see cref="long"/> to a <see cref="JsonElement"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
		/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
		public static JsonElement AsJsonElement(this decimal value)
		{
			var doc = JsonDocument.Parse($"{value}");
			return doc.RootElement;
		}

		/// <summary>
		/// Converts a <see cref="double"/> to a <see cref="JsonElement"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
		/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
		public static JsonElement AsJsonElement(this double value)
		{
			var doc = JsonDocument.Parse($"{value}");
			return doc.RootElement;
		}

		/// <summary>
		/// Converts a <see cref="float"/> to a <see cref="JsonElement"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
		/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
		public static JsonElement AsJsonElement(this float value)
		{
			var doc = JsonDocument.Parse($"{value}");
			return doc.RootElement;
		}

		/// <summary>
		/// Converts a <see cref="string"/> to a <see cref="JsonElement"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
		/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
		public static JsonElement AsJsonElement(this string value)
		{
			var doc = JsonDocument.Parse($"\"{value}\"");
			return doc.RootElement;
		}

		/// <summary>
		/// Converts a <see cref="long"/> to a <see cref="JsonElement"/>.
		/// </summary>
		/// <param name="values">The array of values to convert.</param>
		/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
		/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
		public static JsonElement AsJsonElement(this IEnumerable<JsonElement> values)
		{
			var doc = JsonDocument.Parse($"[{string.Join(",", values.Select(v => v.ToJsonString()))}]");
			return doc.RootElement;
		}

		/// <summary>
		/// Converts a <see cref="long"/> to a <see cref="JsonElement"/>.
		/// </summary>
		/// <param name="values">The value to convert.</param>
		/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
		/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
		public static JsonElement AsJsonElement(this IDictionary<string, JsonElement> values)
		{
			var doc = JsonDocument.Parse($"{{{string.Join(",", values.Select(v => $"\"{v.Key}\":{v.Value.ToJsonString()}"))}}}");
			return doc.RootElement;
		}
	}
}