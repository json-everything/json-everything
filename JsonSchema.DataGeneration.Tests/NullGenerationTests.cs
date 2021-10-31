using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests
{
	public class NullGenerationTests
	{
		[Test]
		public void GenerateNull()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Null);

			Run(schema);
		}
	}
}