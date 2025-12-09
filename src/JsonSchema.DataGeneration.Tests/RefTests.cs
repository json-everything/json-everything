using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestRunner;

namespace Json.Schema.DataGeneration.Tests;

internal class RefTests
{
	[Test]
	public void PointerRef()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = JsonSchema.FromText(
			"""
			{
			    "type": "array",
			    "items": { "$ref": "#/$defs/positiveInteger" },
			    "$defs": {
			        "positiveInteger": {
			            "type": "integer",
			            "exclusiveMinimum": 0
			        }
			    },
			    "minItems": 2
			}
			""", buildOptions);

		Run(schema, buildOptions);
	}

	[Test]
	public void AnchorRef()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = JsonSchema.FromText(
			"""
			{
			    "type": "array",
			    "items": { "$ref": "#positiveInteger" },
			    "$defs": {
			        "positiveInteger": {
			            "$anchor": "positiveInteger",
			            "type": "integer",
			            "exclusiveMinimum": 0
			        }
			    },
			    "minItems": 2
			}
			""", buildOptions);

		Run(schema, buildOptions);
	}

	[Test]
	public void ExternalRef()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var foo = new JsonSchemaBuilder(buildOptions)
			.Id("https://json-everything.test/foo")
			.Type(SchemaValueType.Integer)
			.ExclusiveMinimum(0)
			.Build();

		var schema = JsonSchema.FromText(
			"""
			{
			    "type": "array",
			    "items": { "$ref": "https://json-everything.test/foo" },
			    "minItems": 2
			}
			""", buildOptions);

		Run(schema, buildOptions);
	}

	[Test]
	public void ExternalPointerRef()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var foo = new JsonSchemaBuilder(buildOptions)
			.Id("https://json-everything.test/foo")
			.Defs(
				("positiveInteger", new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.ExclusiveMinimum(0)
				)
			)
			.Build();

		var schema = JsonSchema.FromText(
			"""
			{
			    "type": "array",
			    "items": { "$ref": "https://json-everything.test/foo#/$defs/positiveInteger" },
			    "minItems": 2
			}
			""", buildOptions);

		Run(schema, buildOptions);
	}

	[Test]
	public void ExternalAnchorRef()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var foo = new JsonSchemaBuilder(buildOptions)
			.Id("https://json-everything.test/foo")
			.Defs(
				("positiveInteger", new JsonSchemaBuilder()
					.Anchor("positiveInteger")
					.Type(SchemaValueType.Integer)
					.ExclusiveMinimum(0)
				)
			)
			.Build();

		var schema = JsonSchema.FromText(
			"""
			{
			    "type": "array",
			    "items": { "$ref": "https://json-everything.test/foo#positiveInteger" },
			    "minItems": 2
			}
			""", buildOptions);

		Run(schema, buildOptions);
	}
}