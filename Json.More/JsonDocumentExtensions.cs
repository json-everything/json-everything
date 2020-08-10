using System.Text.Json;

namespace Json.More
{
	public static class JsonDocumentExtensions
	{
		public static bool IsEquivalentTo(this JsonDocument a, JsonDocument b)
		{
			return a.RootElement.IsEquivalentTo(b.RootElement);
		}
	}
}