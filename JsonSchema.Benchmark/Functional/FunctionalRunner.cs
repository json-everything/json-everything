using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Json.Schema.Benchmark.Functional;

[MemoryDiagnoser]
public class FunctionalRunner
{
	private readonly JsonSchema _legacySchema = new JsonSchemaBuilder()
		.Type(SchemaValueType.Number)
		.Minimum(10)
		.Build();

	private readonly JsonNode? _newSchema = JsonNode.Parse(
		"""
		{
		  "type": "number",
		  "minimum": 10
		}
		"""
	);

	private readonly JsonNode? _instance = -5;

	[Params(1,100,10000)]
	public int Count { get; set; }

	[Benchmark]
	public int ObjectOriented()
	{
		int i = 0;

		for (int j = 0; j < Count; j++)
		{
			_legacySchema.Evaluate(_instance);
		}

		return i;
	}

	[Benchmark]
	public int Functional()
	{
		int i = 0;

		for (int j = 0; j < Count; j++)
		{
			JsonSchema.Evaluate(_newSchema, _instance);
		}

		return i;
	}
}