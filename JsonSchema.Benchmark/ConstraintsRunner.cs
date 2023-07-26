using System.Text.Json.Nodes;
using BenchmarkDotNet.Attributes;

namespace Json.Schema.Benchmark;

[MemoryDiagnoser]
public class ConstraintsRunner
{
	[GlobalSetup]
	public void Setup()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Integer);

		var instance = 3;

		_ = schema.Evaluate(instance);

		Schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("value", new JsonSchemaBuilder().Type(SchemaValueType.Integer).Minimum(10)),
				("next", new JsonSchemaBuilder().Ref("#"))
			)
			.AdditionalProperties(new JsonSchemaBuilder().Type(SchemaValueType.String))
			.Build();
		Instance = new JsonObject
		{
			["value"] = 11,
			["next"] = new JsonObject
			{
				["value"] = 12
			},
			["other"] = 13
		};
	}

	[Params(1, 10, 100, 1000)]
	public int RunCount { get; set; }

	public JsonSchema Schema { get; set; }
	public JsonNode Instance { get; set; }

	[Benchmark]
	public void Test()
	{
		for (int i = 0; i < RunCount; i++)
		{
			_ = Schema.EvaluateUsingConstraints(Instance);
		}
	}

	[Benchmark]
	public void Test_Legacy()
	{
		for (int i = 0; i < RunCount; i++)
		{
			_ = Schema.Evaluate(Instance);
		}
	}
}