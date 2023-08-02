using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class DevTest
{
	[Test]
	public void Test()
	{
		var schema = new JsonSchemaBuilder()
			.Ref(MetaSchemas.Draft6Id)
			.Build();
		var instance = new JsonObject
		{
			["items"] = new JsonObject
			{
				["minLength"] = -1
			}
		};

		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical,
			PreserveDroppedAnnotations = true
		};

		var result = schema.Evaluate(instance, options);

		result.AssertInvalid();
	}
}