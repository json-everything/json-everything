using BenchmarkDotNet.Attributes;
using Json.Schema;
using System.Text.Json;
using BenchmarkDotNet.Jobs;

namespace Json.Benchmarks.Schema;

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
		  "required": ["firstName", "lastName", "age"],
		  "additionalProperties": false
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
		      "friends": [
		        {
		          "firstName": "Bob",
		          "lastName": "Johnson",
		          "age": 35,
		          "friends": []
		        }
		      ]
		    },
		    {
		      "firstName": "Alice",
		      "lastName": "Williams",
		      "age": 28,
		      "friends": []
		    }
		  ]
		}
		""";

	private static readonly JsonElement _personInstance;

	static ValidationRunner()
	{
		using var instanceDoc = JsonDocument.Parse(_personInstanceText);
		_personInstance = instanceDoc.RootElement.Clone();
	}

	[Params(1, 10, 100, 1000)]
	public int Count { get; set; }

	[Benchmark]
	public int ValidateWithSchemaCompilation()
	{
		// Compile schema once as part of benchmark (but not repeated)
		using var schemaDoc = JsonDocument.Parse(_personSchemaText);
		var schema = JsonSchema.Build(schemaDoc.RootElement);
		
		var validCount = 0;
		
		// Then validate multiple times
		for (int i = 0; i < Count; i++)
		{
			var result = JsonSchema.Evaluate(schema, _personInstance);
			if (result.IsValid)
				validCount++;
		}

		return validCount;
	}
} 