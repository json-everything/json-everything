using System.IO;
using System.Text.Json.Nodes;
using NUnit.Framework;

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
		var baseSchema = JsonSchema.FromFile(GetFile("base_schema"));
		var refSchema = JsonSchema.FromFile(GetFile("ref_schema"));
		var hashSchema = JsonSchema.FromFile(GetFile("schema_with_#_in_uri"));

		var baseData = JsonNode.Parse(GetResource("base_data"));

		SchemaRegistry.Global.Register(refSchema);
		SchemaRegistry.Global.Register(baseSchema);

		// in previous versions, this would still validate the instance, but since adding
		// static analysis, the ref with the # in it is checked early
		// and since it can't resolve, it now throws.
		Assert.Throws<JsonSchemaException>(() => baseSchema.Evaluate(baseData));
	}

	[Test]
	public void MultipleHashInUriThrowsException()
	{
		var baseSchema = JsonSchema.FromFile(GetFile("base_schema"));
		var refSchema = JsonSchema.FromFile(GetFile("ref_schema"));
		var hashSchema = JsonSchema.FromFile(GetFile("schema_with_#_in_uri"));

		var baseData = JsonNode.Parse(GetResource("base_data_hash_uri"));

		SchemaRegistry.Global.Register(refSchema);
		SchemaRegistry.Global.Register(baseSchema);
		SchemaRegistry.Global.Register(hashSchema);
		
		Assert.Throws<JsonSchemaException>(()=>baseSchema.Evaluate(baseData));
	}

	[Test]
	public void RefIntoMiddleOfResourceToFindDynamicRef()
	{
		var refSchema = new JsonSchemaBuilder()
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
		var schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("schema:local")
			.Ref("schema:ref#/$defs/foo");

		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.List
		};
		options.SchemaRegistry.Register(refSchema);

		var instance = "string";
		var result = schema.Evaluate(instance, options);

		result.AssertInvalid();

	}
}