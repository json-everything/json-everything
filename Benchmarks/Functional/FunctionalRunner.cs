using System.Text.Json.Nodes;
using BenchmarkDotNet.Attributes;

using ExperimentsSchema = Json.Schema.Experiments.JsonSchema;
using ExperimentsOptions = Json.Schema.Experiments.EvaluationOptions;
using ExperimentsResults = Json.Schema.Experiments.EvaluationResults;

namespace Json.Schema.Benchmark.Functional;

[MemoryDiagnoser]
public class FunctionalRunner
{
	private const string _schemaText =
		"""
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "https://benchmark.json-everything.net/schema",
		  "properties": {
		    "foo": {},
		    "bar": {}
		  },
		  "patternProperties": {
		    "^v": {}
		  },
		  "additionalProperties": false
		}
		""";

	private static readonly JsonNode _instance = JsonNode.Parse(
		"""
		{"foo":1,"vroom":2}
		""")!;

	[Params(1,10,50)]
	public int Count { get; set; }

	[Benchmark]
	public int DedicatedModel()
	{
		for (int i = 0; i < Count; i++)
		{
			var schema = JsonSchema.FromText(_schemaText);

			_ = schema.Evaluate(_instance);
		}

		return Count;
	}

	[Benchmark]
	public int DedicatedModel_Reuse()
	{
		var schema = JsonSchema.FromText(_schemaText);

		for (int i = 0; i < Count; i++)
		{
			_ = schema.Evaluate(_instance);
		}

		return Count;
	}

	[Benchmark]
	public int NodeModel()
	{
		for (int i = 0; i < Count; i++)
		{
			var schema = JsonNode.Parse(_schemaText)!;

			_ = ExperimentsSchema.Evaluate(schema, _instance);
		}

		return Count;
	}

	[Benchmark]
	public int NodeModel_Reuse()
	{
		var schema = JsonNode.Parse(_schemaText)!;

		for (int i = 0; i < Count; i++)
		{
			_ = ExperimentsSchema.Evaluate(schema, _instance);
		}

		return Count;
	}
}