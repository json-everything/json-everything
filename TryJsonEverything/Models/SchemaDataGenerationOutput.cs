using System.Text.Json;
using System.Text.Json.Serialization;

namespace TryJsonEverything.Models
{
	public class SchemaDataGenerationOutput
	{
		[JsonPropertyName("result")]
		public JsonElement? Result { get; init; }
		[JsonPropertyName("error")]
		public string? ErrorMessage { get; init; }
	}
}