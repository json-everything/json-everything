﻿using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class SlackTests
{
	[Test]
	public void MultiDraftSelfValidation()
	{
		var json =
			@"{
					""$id"": ""http://localhost/"",
					""$defs"": {
						""M"": {
							""$id"": ""http://localhost/M"",
							""$schema"": ""https://json-schema.org/draft/2020-12/schema"",
							""$defs"": {
								""MarkConfig"": { ""type"": ""integer"" }
							}   
						},  
						""C"": {
							""$id"": ""http://localhost/C"",
							""$schema"": ""http://json-schema.org/draft-06/schema#"",
							""$defs"": {
								""Config"": { ""$ref"": ""http://localhost/M#/$defs/MarkConfig"" }
							},  
							""$ref"": ""http://localhost/C#/$defs/Config""
						}   
					},  
					""$ref"": ""/C""
				}";

		var schema = JsonSchema.FromText(json);
		var instance = JsonNode.Parse(json);

		Assert.Throws<RefResolutionException>(() => schema.Evaluate(instance, new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical
		}));
	}

	[Test]
	public void TypeNonNullAndNullFailsValidation()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("test", new JsonSchemaBuilder()
					.Type(SchemaValueType.Object)
					.AdditionalProperties(new JsonSchemaBuilder()
						.Type(SchemaValueType.String, SchemaValueType.Null)
					)
				)
			)
			.Required("test");

		var instance = new JsonObject
		{
			["test"] = new JsonObject
			{
				["a"] = "aaa",
				["b"] = null
			}
		};

		var results = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		results.AssertValid();
	}
}