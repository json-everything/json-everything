using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class DevTest
{
	[Test]
	public void Test()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchema[]
			{
				new JsonSchemaBuilder().Type(SchemaValueType.Integer).Minimum(10),
				new JsonSchemaBuilder().Type(SchemaValueType.Integer).Minimum(20)
			})
			.AdditionalItems(new JsonSchemaBuilder().Type(SchemaValueType.String))
			.Build();
		var instance = new JsonArray { 15, 25, 1 };

		var result = schema.EvaluateUsingConstraints(instance, new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical,
			PreserveDroppedAnnotations = true
		});

		result.AssertInvalid();
	}
}