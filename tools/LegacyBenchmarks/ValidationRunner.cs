using BenchmarkDotNet.Attributes;
using Json.Schema;
using System.Text.Json;
using BenchmarkDotNet.Jobs;

namespace Json.LegacyBenchmarks;

[MemoryDiagnoser]
//[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class ValidationRunner
{
	private const string _personSchemaText =
		"""
		{
		  "type": "object",
		  "properties": {
		    "firstName": {
		      "type": "string"
		    },
		    "lastName": {
		      "type": "string"
		    },
		    "age": {
		      "type": "integer",
		      "minimum": 0
		    },
		    "friends": {
		      "type": "array",
		      "items": {
		        "$ref": "#"
		      }
		    }
		  },
		  "required": ["firstName", "lastName", "age"]
		}
		""";

	private const string _personInstanceText =
		"""
		{
		  "firstName": "John",
		  "lastName": "Doe",
		  "age": 30,
		  "friends": [
		    {
		      "firstName": "Jane",
		      "lastName": "Smith",
		      "age": 25,
		      "friends": []
		    },
		    {
		      "firstName": "Bob",
		      "lastName": "Johnson",
		      "age": 35,
		      "friends": [
		        {
		          "firstName": "Alice",
		          "lastName": "Brown",
		          "age": 28,
		          "friends": []
		        }
		      ]
		    }
		  ]
		}
		""";

	[Params(1, 10, 100, 1000)]
	public int Count { get; set; }

	[Benchmark]
	public bool LegacySchemaValidation()
	{
		// Parse the schema once
		var schema = JsonSchema.FromText(_personSchemaText);
		
		// Parse the instance once
		using var document = JsonDocument.Parse(_personInstanceText);
		var instance = document.RootElement;
		
		// Run validation Count times
		bool result = false;
		for (int i = 0; i < Count; i++)
		{
			var evaluationResults = schema.Evaluate(instance);
			result = evaluationResults.IsValid;
		}
		
		return result;
	}
} 