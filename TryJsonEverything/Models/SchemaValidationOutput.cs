using System.Text.Json.Serialization;
using Json.Schema;

namespace TryJsonEverything.Models
{
	public class SchemaValidationOutput
	{
		[JsonPropertyName("result")]
		public ValidationResults Result { get; set; }
	}
}