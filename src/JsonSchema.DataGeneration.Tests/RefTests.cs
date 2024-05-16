using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestRunner;

namespace Json.Schema.DataGeneration.Tests;

internal class RefTests
{
	[Test]
	public void PointerRef()
	{
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
			""");

		Run(schema);
	}

	[Test]
	public void AnchorRef()
	{
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
			""");

		Run(schema);
	}

	[Test]
	public void ExternalRef()
	{
		var foo = new JsonSchemaBuilder()
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
			""");

		var options = new EvaluationOptions();
		options.SchemaRegistry.Register(foo);

		Run(schema, options);
	}

	[Test]
	public void ExternalPointerRef()
	{
		var foo = new JsonSchemaBuilder()
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
			""");

		var options = new EvaluationOptions();
		options.SchemaRegistry.Register(foo);

		Run(schema, options);
	}

	[Test]
	public void ExternalAnchorRef()
	{
		var foo = new JsonSchemaBuilder()
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
			""");

		var options = new EvaluationOptions();
		options.SchemaRegistry.Register(foo);

		Run(schema, options);
	}
}