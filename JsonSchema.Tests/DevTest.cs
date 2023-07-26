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
			.Type(SchemaValueType.Object)
			.Properties(
				("value", new JsonSchemaBuilder().Type(SchemaValueType.Integer).Minimum(10)),
				("next", new JsonSchemaBuilder().Ref("#"))
			)
			.AdditionalProperties(new JsonSchemaBuilder().Type(SchemaValueType.String))
			.Build();
		var instance = new JsonObject
		{
			["value"] = 11,
			["next"] = new JsonObject
			{
				["value"] = 12
			},
			["other"] = 13
		};

		var result = schema.Evaluate2(instance, new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical,
			PreserveDroppedAnnotations = true
		});

		result.AssertInvalid();
	}
}