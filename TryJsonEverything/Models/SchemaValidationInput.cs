using System.Text.Json;
using Json.Schema;

namespace TryJsonEverything.Models
{
	public class SchemaValidationStringInput
	{
		public string Instance { get; set; }
		public string Schema { get; set; }
		public OutputFormat OutputFormat { get; set; }
	}

	public class SchemaVlidationInput
	{
		public JsonDocument Instance { get; set; }
		public JsonSchema Schema { get; set; }
		public OutputFormat OutputFormat { get; set; }
	}
}
