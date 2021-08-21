using Json.Schema;

namespace TryJsonEverything.Models
{
	public class SchemaValidationOutput
	{
		public ValidationResults MetaSchemaResult { get; set; }
		public ValidationResults Result { get; set; }
	}
}