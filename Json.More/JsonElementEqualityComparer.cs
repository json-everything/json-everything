using System.Collections.Generic;
using System.Text.Json;

namespace Json.More
{
	public class JsonElementEqualityComparer : IEqualityComparer<JsonElement>
	{
		public static JsonElementEqualityComparer Instance { get; } = new JsonElementEqualityComparer();

		public bool Equals(JsonElement x, JsonElement y)
		{
			return x.IsEquivalentTo(y);
		}

		public int GetHashCode(JsonElement obj)
		{
			return obj.GetHashCode();
		}
	}
}