using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests
{
	public class SerializationTests
	{
		[TestCase("true")]
		[TestCase("false")]
		[TestCase("{\"$id\":\"http://some.site/schema\"}")]
		[TestCase("{\"$schema\":\"http://some.site/schema\"}")]
		[TestCase("{\"$ref\":\"#/json/pointer\"}")]
		[TestCase("{\"title\":\"some text\"}")]
		[TestCase("{\"description\":\"some text\"}")]
		[TestCase("{\"default\":\"some text\"}")]
		[TestCase("{\"default\":9}")]
		[TestCase("{\"default\":9.0}")]
		[TestCase("{\"default\":true}")]
		[TestCase("{\"default\":false}")]
		[TestCase("{\"default\":null}")]
		[TestCase("{\"default\":[]}")]
		[TestCase("{\"default\":{}}")]
		[TestCase("{\"examples\":[1,true,false,null,\"string\",[],{}]}")]
		[TestCase("{\"multipleOf\":1}")]
		[TestCase("{\"maximum\":1}")]
		[TestCase("{\"exclusiveMaximum\":1}")]
		[TestCase("{\"minimum\":1}")]
		[TestCase("{\"exclusiveMinimum\":1}")]
		[TestCase("{\"maxLength\":1}")]
		[TestCase("{\"minLength\":1}")]
		[TestCase("{\"pattern\":\"$yes{1,3}^\"}")]
		[TestCase("{\"additionalItems\":true}")]
		[TestCase("{\"additionalItems\":false}")]
		[TestCase("{\"additionalItems\":{\"$id\":\"http://some.site/schema\"}}")]
		public void RoundTrip(string text)
		{
			var schema = JsonSchema.FromText(text);

			var returnToText = JsonSerializer.Serialize(schema);

			Assert.AreEqual(text, returnToText);
		}
	}
}