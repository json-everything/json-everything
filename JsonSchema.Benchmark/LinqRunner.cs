using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using BenchmarkDotNet.Attributes;
using Json.More;

namespace Json.Schema.Benchmark;

[MemoryDiagnoser]
public class LinqRunner
{
	private static readonly JsonSchema _schema =
		new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft7Id)
			.PatternProperties(
				("^abc", new JsonSchemaBuilder().Type(SchemaValueType.String)),
				("^123", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			)
			.AdditionalProperties(new JsonSchemaBuilder().Type(SchemaValueType.Boolean));

	private static readonly JsonNode _instance =
		new JsonObject
		{
			["abc2"] = "another string",
			["wewvena"] = true,
			["abc1"] = "a string",
			["1235"] = 1235,
			["wevoina"] = false,
			["1234"] = 1234,
			["onvoinw"] = false
		};

	[Params(10, 100, 1000, 10000)]
	public static int Iterations { get; set; }

	[Params(true, false)]
	public static bool RunAsBase { get; set; }

	[Benchmark]
	public int Run()
	{
		BenchmarkSettings.RunAsBase = RunAsBase;

		for (int i = 0; i < Iterations; i++)
		{
			_ = _schema.Evaluate(_instance);
		}

		return Iterations;
	}
}