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
			.Defs(
				("foo", new JsonSchemaBuilder().Minimum(20))
			)
			.Properties(
				("foo", new JsonSchemaBuilder().Minimum(20))
			)
			.Build();
		var instance = new JsonObject
		{
			["foo"] = 15
		};

		var result = schema.Evaluate2(instance, new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical,
			PreserveDroppedAnnotations = true
		});

		result.AssertInvalid();
	}
}