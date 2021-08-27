using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using Json.Schema;
using TryJsonEverything.Services;

namespace TryJsonEverything.Models
{
	[Schema(typeof(InputValidationSchemas), nameof(InputValidationSchemas.SchemaInputSchema))]
	public class SchemaValidationInput
	{
		public JsonDocument Instance { get; set; }
		public JsonSchema Schema { get; set; }
		public OutputFormat OutputFormat { get; set; }
	}
}
