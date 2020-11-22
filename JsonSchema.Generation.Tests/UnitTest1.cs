using NUnit.Framework;

namespace Json.Schema.Generation.Tests
{
	public class SimpleTypeTests
	{
		[Test]
		public void BooleanSchema()
		{
			JsonSchema expected = new JsonSchemaBuilder().Type(SchemaValueType.Boolean);
				
			JsonSchema actual = new JsonSchemaBuilder().FromType<bool>();

			Assert.AreEqual(expected, actual);
		}
	}
}