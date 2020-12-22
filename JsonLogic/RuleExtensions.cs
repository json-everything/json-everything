using System.Text.Json;

namespace Json.Logic
{
	public static class RuleExtensions
	{
		public static JsonElement Apply(this Rule component)
		{
			return component.Apply(JsonDocument.Parse("null").RootElement);
		}
	}
}