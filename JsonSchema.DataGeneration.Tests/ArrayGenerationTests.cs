using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests
{
	public class ArrayGenerationTests
	{
		[Test]
		public void GenerateArrayOfNumbers()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Array)
				.Items(new JsonSchemaBuilder()
					.Type(SchemaValueType.Number)
					.Minimum(10)
					.Maximum(20))
				.MinItems(3)
				.MaxItems(10);

			Run(schema);
		}
	}
}
