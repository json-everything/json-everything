using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.More
{
	public static class JsonElementExtensions
	{
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

		public static JsonElement AsJsonElement(this long value)
		{
			var doc = JsonDocument.Parse($"{value}");
			return doc.RootElement;
		}

		public static JsonElement AsJsonElement(this int value)
		{
			var doc = JsonDocument.Parse($"{value}");
			return doc.RootElement;
		}

		public static JsonElement AsJsonElement(this short value)
		{
			var doc = JsonDocument.Parse($"{value}");
			return doc.RootElement;
		}

		public static JsonElement AsJsonElement(this bool value)
		{
			var doc = JsonDocument.Parse($"{value.ToString().ToLower()}");
			return doc.RootElement;
		}

		public static JsonElement AsJsonElement(this decimal value)
		{
			var doc = JsonDocument.Parse($"{value}");
			return doc.RootElement;
		}

		public static JsonElement AsJsonElement(this double value)
		{
			var doc = JsonDocument.Parse($"{value}");
			return doc.RootElement;
		}

		public static JsonElement AsJsonElement(this float value)
		{
			var doc = JsonDocument.Parse($"{value}");
			return doc.RootElement;
		}

		public static JsonElement AsJsonElement(this string value)
		{
			var doc = JsonDocument.Parse($"\"{value}\"");
			return doc.RootElement;
		}

		public static JsonElement AsJsonElement(this IEnumerable<JsonElement> values)
		{
			var doc = JsonDocument.Parse($"\"{values}\"");
			return doc.RootElement;
		}

		public static JsonElement AsJsonElement(this IDictionary<string, JsonElement> values)
		{
			var doc = JsonDocument.Parse($"\"{values}\"");
			return doc.RootElement;
		}
	}
}