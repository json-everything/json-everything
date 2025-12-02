using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestRunner;

namespace Json.Schema.DataGeneration.Tests;

public class DevTests
{
	[Test]
	public void Test()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Number)
			.Minimum(0)
			.Maximum(100)
			.Not(new JsonSchemaBuilder().MultipleOf(3));

		Run(schema, buildOptions);
	}
}