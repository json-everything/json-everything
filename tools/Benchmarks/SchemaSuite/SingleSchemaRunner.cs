using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Json.Schema;

namespace Json.Benchmarks.SchemaSuite;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, baseline:true)]
[SimpleJob(RuntimeMoniker.Net90)]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class SingleSchemaRunner
{
	private static readonly JsonElement _schema = 
		JsonDocument.Parse("""
			{
			  "$schema": "https://json-schema.org/draft/2020-12/schema",
			  "type": "object",
			  "properties": {
			    "users": {
			      "type": "array",
			      "uniqueItems": true,
			      "contains": {
			        "type": "object",
			        "required": ["role"],
			        "properties": {
			          "role": { "const": "admin" }
			        }
			      },
			      "minContains": 1,
			      "maxContains": 3,
			      "items": {
			        "type": "object",
			        "properties": {
			          "id": { "type": "integer" },
			          "email": { "type": "string" },
			          "role": { "type": "string" }
			        }
			      }
			    },
			    "products": {
			      "type": "array",
			      "items": {
			        "type": "object",
			        "properties": {
			          "sku": { "type": "string" },
			          "price": { "type": "number" },
			          "category": { "type": "string" }
			        }
			      }
			    },
			    "metadata": {
			      "type": "object",
			      "properties": {
			        "version": { "type": "string" },
			        "timestamp": { "type": "integer" }
			      },
			      "patternProperties": {
			        "^x-": { "type": "string" },
			        "^app-": { "type": "object" },
			        "^meta-": { "type": "number" }
			      },
			      "additionalProperties": false
			    }
			  },
			  "allOf": [
			    {
			      "if": {
			        "properties": {
			          "users": {
			            "type": "array",
			            "minItems": 10
			          }
			        }
			      },
			      "then": {
			        "properties": {
			          "metadata": {
			            "required": ["version"]
			          }
			        }
			      }
			    }
			  ],
			  "dependentSchemas": {
			    "users": {
			      "required": ["metadata"]
			    },
			    "products": {
			      "properties": {
			        "metadata": {
			          "required": ["timestamp"]
			        }
			      }
			    }
			  }
			}
			""").RootElement;

	private static readonly JsonElement _instance =
		JsonDocument.Parse("""
			{
			  "users": [
			    {"id": 1, "email": "user1@example.com", "role": "admin"},
			    {"id": 2, "email": "user2@example.com", "role": "user"},
			    {"id": 3.0, "email": "user3@example.com", "role": "user"},
			    {"id": 4, "email": "user4@example.com", "role": "moderator"},
			    {"id": 5, "email": "user5@example.com", "role": "user"},
			    {"id": 6, "email": "user6@example.com", "role": "admin"},
			    {"id": 7, "email": "user7@example.com", "role": "user"},
			    {"id": 8, "email": "user8@example.com", "role": "user"},
			    {"id": 9, "email": "user9@example.com", "role": "user"},
			    {"id": 10, "email": "user10@example.com", "role": "admin"}
			  ],
			  "products": [
			    {"sku": "PROD-001", "price": 29.99, "category": "electronics"},
			    {"sku": "PROD-002", "price": 49.99, "category": "books"},
			    {"sku": "PROD-003", "price": 19.99, "category": "electronics"},
			    {"sku": "PROD-004", "price": 99.99, "category": "furniture"},
			    {"sku": "PROD-005", "price": 15.99, "category": "books"}
			  ],
			  "metadata": {
			    "version": "2.1.0",
			    "timestamp": 1702234567,
			    "x-custom-header": "value1",
			    "x-another-header": "value2",
			    "app-config": {"enabled": true},
			    "app-settings": {"theme": "dark"},
			    "meta-count": 42,
			    "meta-score": 87.5
			  }
			}
			""").RootElement;

	private static readonly BuildOptions _buildOptions = new()
	{
		Dialect = Dialect.Draft201909
	};

	//[Benchmark]
	//[Arguments(1)]
	//[Arguments(5)]
	//[Arguments(10)]
	//[Arguments(50)]
	public int BuildAlways(int n)
	{
		for (int j = 0; j < n; j++)
		{
			var schema = JsonSchema.Build(_schema, _buildOptions);
			_ = schema.Evaluate(_instance);
		}

		return n;
	}

	[Benchmark]
	//[Arguments(1)]
	[Arguments(5)]
	[Arguments(10)]
	[Arguments(50)]
	public int BuildOnce(int n)
	{
		var schema = JsonSchema.Build(_schema, _buildOptions);
		for (int j = 0; j < n; j++)
		{
			_ = schema.Evaluate(_instance);
		}

		return n;
	}
}