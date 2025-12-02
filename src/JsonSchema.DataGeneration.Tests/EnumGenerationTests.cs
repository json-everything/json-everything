using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestRunner;

namespace Json.Schema.DataGeneration.Tests;

public class EnumGenerationTests
{
	[Test]
	public void EnumPicksAValue()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Enum("these", "are", "all", "the", "options");

		Run(schema, buildOptions);
	}
}