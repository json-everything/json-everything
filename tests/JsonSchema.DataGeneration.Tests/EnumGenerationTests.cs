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

		Run(schema);
	}

	[Test]
	public void EnumSingleValue()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Enum("only");

		Run(schema);
	}

	[Test]
	public void EnumMixedTypes()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Enum(1, "two", true, null);

		Run(schema);
	}
}