using System.Collections.Generic;
using System.Text.Json;

namespace Json.More
{
	public class JsonDocumentEqualityComparer : IEqualityComparer<JsonDocument>
	{
		public static JsonDocumentEqualityComparer Instance { get; } = new JsonDocumentEqualityComparer();

		public bool Equals(JsonDocument x, JsonDocument y)
		{
			return x.IsEquivalentTo(y);
		}

		public int GetHashCode(JsonDocument obj)
		{
			return obj.GetHashCode();
		}
	}
}