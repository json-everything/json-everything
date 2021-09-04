using System.Text.Json;
using Json.Path;
using Json.Schema;

namespace TryJsonEverything.Services
{
	public static class CustomFormats
	{
		public static readonly Format JsonPath = new PredicateFormat("json-path", ValidateJsonPath);

		static CustomFormats()
		{
			Formats.Register(JsonPath);
		}

		private static bool ValidateJsonPath(JsonElement element, out string? errorMessage)
		{
			if (element.ValueKind != JsonValueKind.String)
			{
				errorMessage = "Value must be a string";
				return false;
			}

			try
			{
				Json.Path.JsonPath.Parse(element.GetString()!);
				errorMessage = null;
				return true;
			}
			catch (PathParseException e)
			{
				errorMessage = $"{e.Message} at index {e.Index}";
				return false;
			}
		}
	}
}
