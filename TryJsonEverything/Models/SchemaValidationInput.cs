using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using Json.Schema;

namespace TryJsonEverything.Models
{
	public class SchemaValidationInput
	{
		public JsonDocument Instance { get; set; }
		public JsonSchema Schema { get; set; }
		public OutputFormat OutputFormat { get; set; }
	}
}
