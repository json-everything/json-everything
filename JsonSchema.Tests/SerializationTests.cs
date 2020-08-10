using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests
{
	public class SerializationTests
	{
		[TestCase("{\"title\":\"some text\"}")]
		[TestCase("{\"default\":\"some text\"}")]
		[TestCase("{\"default\":9}")]
		[TestCase("{\"default\":9.0}")]
		[TestCase("{\"default\":true}")]
		[TestCase("{\"default\":false}")]
		[TestCase("{\"default\":null}")]
		[TestCase("{\"default\":[]}")]
		[TestCase("{\"default\":{}}")]
		[TestCase("{\"$id\":\"http://some.site/schema\"}")]
		public void RoundTrip(string text)
		{
			var schema = JsonSchema.FromText(text);

			var returnToText = JsonSerializer.Serialize(schema);

			Assert.AreEqual(text, returnToText);
		}
	}
}