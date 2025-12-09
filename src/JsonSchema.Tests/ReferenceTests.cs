using System.IO;
using System.Text.Json;
using Json.More;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.Tests;

public class ReferenceTests
{
	private static string GetFile(string name)
	{
		return Path.Combine(TestContext.CurrentContext.WorkDirectory, "Files", "Referencing", $"{name}.json")
			.AdjustForPlatform();
	}

	private static string GetResource(string name)
	{
		return File.ReadAllText(GetFile(name));
	}

	[Test]
	public void ReferenceFragmentFromFile()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };

		var baseSchema = JsonSchema.FromFile(GetFile("base_schema"), buildOptions);
		_ = JsonSchema.FromFile(GetFile("ref_schema"), buildOptions);
		_ = JsonSchema.FromFile(GetFile("schema_with_#_in_uri"), buildOptions);

		var baseData = JsonDocument.Parse(GetResource("base_data")).RootElement;

		var options = new EvaluationOptions();

		// in previous versions, this would still validate the instance, but since adding
		// static analysis, the ref with the # in it is checked early
		// and since it can't resolve, it now throws.
		Assert.Throws<RefResolutionException>(() => baseSchema.Evaluate(baseData, options));
	}

	[Test]
	public void MultipleHashInUriThrowsException()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
	
		var baseSchema = JsonSchema.FromFile(GetFile("base_schema"), buildOptions);
		_ = JsonSchema.FromFile(GetFile("ref_schema"), buildOptions);
		_ = JsonSchema.FromFile(GetFile("schema_with_#_in_uri"), buildOptions);

		var baseData = JsonDocument.Parse(GetResource("base_data_hash_uri")).RootElement;

		var options = new EvaluationOptions();

		Assert.Throws<RefResolutionException>(()=>baseSchema.Evaluate(baseData, options));
	}

	[Test]
	public void RefIntoMiddleOfResourceToFindDynamicRef()
	{
		var buildOptions = new BuildOptions
		{
			SchemaRegistry = new(),
			Dialect = Dialect.Draft202012
		};

		_ = new JsonSchemaBuilder(buildOptions)
			.Schema(MetaSchemas.Draft202012Id)
			.Id("schema:ref")
			.Defs(
				("foo", new JsonSchemaBuilder().DynamicRef("#detached")),
				("detached", new JsonSchemaBuilder()
					.DynamicAnchor("detached")
					.Type(SchemaValueType.Integer)
				)
			)
			.Build();
		var schema = new JsonSchemaBuilder(buildOptions)
			.Schema(MetaSchemas.Draft202012Id)
			.Id("schema:local")
			.Ref("schema:ref#/$defs/foo");

		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.List
		};

		var instance = "string".AsJsonElement();
		var result = schema.Evaluate(instance, options);

		result.AssertInvalid();

	}
}