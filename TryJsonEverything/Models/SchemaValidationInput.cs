using System.Text.Json;
using Json.Schema;
using TryJsonEverything.Services;
#pragma warning disable 8618

namespace TryJsonEverything.Models
{
	[Schema(typeof(InputValidationSchemas), nameof(InputValidationSchemas.SchemaInputSchema))]
	public class SchemaValidationInput
	{
		public JsonDocument Instance { get; set; }
		public JsonSchema Schema { get; set; }
		public ValidationOptionsInput? Options { get; set; }
	}

	[Schema(typeof(InputValidationSchemas), nameof(InputValidationSchemas.SchemaDataGenerationInputSchema))]
	public class SchemaDataGenerationInput
	{
		public JsonSchema Schema { get; set; }
	}
}
