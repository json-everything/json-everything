using System.Text.Json;
using Json.Schema;

namespace TryJsonEverything.Services
{
	public static class CustomFormats
	{
		public static readonly Format JsonPath = new PredicateFormat("json-path", e => e.ValueKind == JsonValueKind.String &&
		                                                                               Json.Path.JsonPath.TryParse(e.GetString()!, out _));

		static CustomFormats()
		{
			Formats.Register(JsonPath);
		}
	}
}
