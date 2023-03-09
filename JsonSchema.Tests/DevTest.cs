using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class DevTest
{
	[Test]
	public void Test()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("example-schema")
			.Type(SchemaValueType.Object);

		var instance = new JsonObject { ["foo"] = "test" };

		var results = schema.Evaluate(instance);

		results.AssertValid();
	}
}