using System.Text.Json;

namespace Json.Logic
{
	public static class LogicComponentExtensions
	{
		public static JsonElement Apply(this LogicComponent component)
		{
			return component.Apply(JsonDocument.Parse("null").RootElement);
		}
	}
}