using System.Text.Json.Serialization;
using Json.Schema;
#pragma warning disable 8618

namespace TryJsonEverything.Models
{
	public class SchemaValidationOutput
	{
		[JsonPropertyName("result")]
		public ValidationResults Result { get; init; }
	}
}