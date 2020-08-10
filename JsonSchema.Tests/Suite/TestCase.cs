using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema.Tests.Suite
{
	public class TestCase
	{
		public string Description { get; set; }
		[JsonConverter(typeof(EmbeddedDataJsonConverter))]
		public JsonElement Data { get; set; }
		public bool Valid { get; set; }
	}
}