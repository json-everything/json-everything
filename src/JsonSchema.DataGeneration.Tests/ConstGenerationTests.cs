using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestRunner;

namespace Json.Schema.DataGeneration.Tests;

public class ConstGenerationTests
{
	[Test]
	public void ConstSchemaGeneratesItsValue()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Const("this is totally a random string");

		Run(schema, buildOptions);
	}

	[Test]
	public void NotConstSchemaGeneratesAnythingButItsValue()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Not(new JsonSchemaBuilder()
				.Const("anything but this value"));

		Run(schema, buildOptions);
	}
}