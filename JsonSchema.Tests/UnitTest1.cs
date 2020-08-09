using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests
{
	public class Tests
	{
		[Test]
		public void Test1()
		{
			var schema = JsonSchema.FromText("{\"$id\":\"http://my.schema/test1\",\"minimum\":5}");

			var json = JsonDocument.Parse("10");

			var results = schema.Validate(json.RootElement);

			Assert.True(results.IsValid);
		}
	}

	public class JsonSchemaTestSuite
	{
		[OneTimeSetUp]
		public void GetTests()
		{

		}
	}
}