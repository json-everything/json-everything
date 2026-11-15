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

		Run(schema);
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

		Run(schema);
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

		Run(schema);
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

		Run(schema);
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

		Run(schema);
	}

	[Test]
	public void RecursiveLinkedListRef()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = JsonSchema.FromText(
			"""
			{
			  "$ref": "#/$defs/node",
			  "$defs": {
			    "node": {
			      "type": ["object", "null"],
			      "properties": {
			        "value": { "type": "integer" },
			        "next": { "$ref": "#/$defs/node" }
			      },
			      "required": ["value", "next"],
			      "additionalProperties": false
			    }
			  }
			}
			""", buildOptions);

		Run(schema);
	}

	[Test]
	public void RecursiveMutualRef()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = JsonSchema.FromText(
			"""
			{
			  "$ref": "#/$defs/a",
			  "$defs": {
			    "a": {
			      "type": ["object", "null"],
			      "properties": {
			        "value": { "type": "integer" },
			        "next": { "$ref": "#/$defs/b" }
			      },
			      "required": ["value", "next"],
			      "additionalProperties": false
			    },
			    "b": {
			      "type": ["object", "null"],
			      "properties": {
			        "value": { "type": "integer" },
			        "next": { "$ref": "#/$defs/a" }
			      },
			      "required": ["value", "next"],
			      "additionalProperties": false
			    }
			  }
			}
			""", buildOptions);

		Run(schema);
	}

	[Test]
	public void DynamicRefResolvesByEntryPoint()
	{
		var buildOptions = new BuildOptions
		{
			SchemaRegistry = new(),
			Dialect = Dialect.Draft202012
		};

		_ = JsonSchema.FromText(
			"""
			{
			  "$schema": "https://json-schema.org/draft/2020-12/schema",
			  "$id": "https://json-everything.test/tree-core",
			  "$defs": {
			    "node": {
			      "$dynamicRef": "#nodeType"
			    },
			    "default": {
			      "$dynamicAnchor": "nodeType",
			      "not": true
			    }
			  },
			  "$ref": "#/$defs/node"
			}
			""", buildOptions);

		var integerEntry = JsonSchema.FromText(
			"""
			{
			  "$schema": "https://json-schema.org/draft/2020-12/schema",
			  "$id": "https://json-everything.test/entry-int",
			  "$defs": {
			    "intNode": {
			      "$dynamicAnchor": "nodeType",
			      "type": "integer",
			      "minimum": 0
			    }
			  },
			  "$ref": "https://json-everything.test/tree-core"
			}
			""", buildOptions);

		var stringEntry = JsonSchema.FromText(
			"""
			{
			  "$schema": "https://json-schema.org/draft/2020-12/schema",
			  "$id": "https://json-everything.test/entry-string",
			  "$defs": {
			    "stringNode": {
			      "$dynamicAnchor": "nodeType",
			      "type": "string",
			      "minLength": 1
			    }
			  },
			  "$ref": "https://json-everything.test/tree-core"
			}
			""", buildOptions);

		Run(integerEntry);
		Run(stringEntry);
	}
}