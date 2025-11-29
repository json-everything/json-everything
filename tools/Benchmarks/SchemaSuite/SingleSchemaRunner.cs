using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Json.Schema;

namespace Json.Benchmarks.SchemaSuite;

[MemoryDiagnoser]
//[SimpleJob(RuntimeMoniker.Net80, baseline:true)]
[SimpleJob(RuntimeMoniker.Net90)]
//[SimpleJob(RuntimeMoniker.Net10_0)]
public class SingleSchemaRunner
{
	private static readonly JsonElement _schema = 
		JsonDocument.Parse("""
			{
			  "$schema": "https://json-schema.org/draft/2019-09/schema",
			  "items": [
			    {
			      "type": "string"
			    }
			  ],
			  "allOf": [
			    {
			      "items": [
			        true,
			        {
			          "type": "number"
			        }
			      ]
			    }
			  ],
			  "unevaluatedItems": false
			}
			""").RootElement;

	private static readonly JsonElement _instance =
		JsonDocument.Parse("""
			[
			  "foo",
			  42,
			  true
			]
			""").RootElement;

	private static readonly BuildOptions _buildOptions = new()
	{
		Dialect = Dialect.Draft201909
	};

	//[Benchmark]
	[Arguments(1)]
	//[Arguments(5)]
	//[Arguments(10)]
	//[Arguments(50)]
	public int BuildAlways(int n)
	{
		int i = 0;

		for (int j = 0; j < n; j++)
		{
			var schema = JsonSchema.Build(_schema, _buildOptions);
			_ = schema.Evaluate(_instance);
		}
		i++;

		return i;
	}

	[Benchmark]
	[Arguments(1)]
	//[Arguments(5)]
	[Arguments(10)]
	[Arguments(50)]
	public int BuildOnce(int n)
	{
		int i = 0;
		var schema = JsonSchema.Build(_schema, _buildOptions);
		for (int j = 0; j < n; j++)
		{
			_ = schema.Evaluate(_instance);
		}
		i++;

		return i;
	}
}