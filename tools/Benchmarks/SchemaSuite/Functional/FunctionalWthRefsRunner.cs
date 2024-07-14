using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using BenchmarkDotNet.Attributes;
using Json.Schema;

namespace Json.Benchmarks.SchemaSuite.Functional;

[MemoryDiagnoser]
public class FunctionalWthRefsRunner
{
	private const string _schemaText =
		"""
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "https://benchmark.json-everything.net/schema",
		  "properties": {
			"key": { "enum": [] }
		  },
		  "required": [ "key" ],
		  "oneOf": [
		  ],
		  "$defs": {
		  }
		}
		""";

	private static readonly JsonNode _instance = JsonNode.Parse(
		"""
		{
		  "type": "bar",
		  "bar": {
		    "type": "null"
		  }
		}
		""")!;

	private static readonly Dictionary<int, (JsonNode AsNode, JsonSchema AsSchema)> _schemas = new();

	[Params(1,10,50)]
	public int ValidationCount { get; set; }

	[Params(1,5,10,50)]
	public int OptionCount { get; set; }

	[GlobalSetup]
	public void BenchmarkSetup()
	{
		(JsonNode, JsonSchema) CreateEntry(int count)
		{
			var node = BuildSchema(count);
			var schema = node.Deserialize<JsonSchema>()!;

			return (node, schema);
		}

		_schemas[1] = CreateEntry(1);
		_schemas[5] = CreateEntry(5);
		_schemas[10] = CreateEntry(10);
		_schemas[20] = CreateEntry(20);
		_schemas[30] = CreateEntry(30);
		_schemas[40] = CreateEntry(40);
		_schemas[50] = CreateEntry(50);

		EvaluationOptions.Default.EnableExperiments();
	}

	private static JsonNode BuildSchema(int optionCount)
	{
		var schema = JsonNode.Parse(_schemaText);

		var enumNode = schema["properties"]["key"]["enum"].AsArray();
		var oneOfNode = schema["oneOf"].AsArray();
		var defsNode = schema["$defs"].AsObject();
		for (int i = 0; i < optionCount; i++)
		{
			enumNode.Add(i);
			var oneOfText = $$"""
                {
                  "if": {
                    "properties": { "key": { "const": {{i}} } }
                  },
                  "then": { "$ref": "#/$defs/{{i}}" }
                }
                """;
			oneOfNode.Add(JsonNode.Parse(oneOfText));
			var defsText = $$"""
                {
                  "properties": {
                    "{{i}}": { "$ref": "#" }
                  },
                  "required": ["{{i}}"]
                }
                """;
			defsNode.Add(i.ToString(), JsonNode.Parse(defsText));
		}

		return schema;
	}

	[Benchmark]
	public int DedicatedModel()
	{
		var schema = _schemas[OptionCount].AsSchema;
		
		for (int i = 0; i < ValidationCount; i++)
		{
			_ = schema.Evaluate(_instance);
		}

		return ValidationCount;
	}

	[Benchmark]
	public int NodeModel()
	{
		var schema = _schemas[OptionCount].AsNode;
	
		for (int i = 0; i < ValidationCount; i++)
		{
			_ = JsonSchema.Evaluate(schema, _instance);
		}

		return ValidationCount;
	}
}