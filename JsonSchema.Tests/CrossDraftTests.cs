using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class CrossDraftTests
{
	[Test]
	public void Test()
	{
		var schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("base")
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", new JsonSchemaBuilder().Ref("foo"))
			)
			.Defs(
				("foo-def", new JsonSchemaBuilder()
					.Schema(MetaSchemas.Draft7Id)
					.Id("foo")
					.Type(SchemaValueType.Array)
					.Items(new JsonSchema[]
					{
						new JsonSchemaBuilder().Type(SchemaValueType.String),
						new JsonSchemaBuilder().Type(SchemaValueType.Integer)
					})
				)
			)
			.Build();
		var instance = new JsonObject
		{
			["foo"] = new JsonArray { "string", 42 }
		};

		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical
		};

		var result = schema.Evaluate(instance, options);

		result.AssertValid();
	}
}