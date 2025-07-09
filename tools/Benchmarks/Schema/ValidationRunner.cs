using BenchmarkDotNet.Attributes;
using Json.Schema;
using System.Text.Json;
using BenchmarkDotNet.Jobs;

using AstSchema = Json.Schema.Ast.JsonSchema;

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
	public int Count { get; set; } = 1;

	[Benchmark]
	public bool NewSchemaValidation()
	{
		// Parse and build the schema once
		using var schemaDoc = JsonDocument.Parse(_personSchemaText);
		var schema = JsonSchema.Build(schemaDoc.RootElement);
		
		// Parse the instance once
		using var instanceDoc = JsonDocument.Parse(_personInstanceText);
		var instance = instanceDoc.RootElement;
		
		// Run validation Count times
		bool result = false;
		for (int i = 0; i < Count; i++)
		{
			var evaluationResults = JsonSchema.Evaluate(schema, instance);
			result = evaluationResults.IsValid;
		}
		
		return result;
	}

	//[Benchmark]
	public bool NewSchemaValidation2()
	{
		// Parse and build the schema once
		using var schemaDoc = JsonDocument.Parse(_personSchemaText);
		var schema = JsonSchema.Build(schemaDoc.RootElement);
		
		// Parse the instance once
		using var instanceDoc = JsonDocument.Parse(_personInstanceText);
		var instance = instanceDoc.RootElement;
		
		// Run validation Count times using Evaluate2 (hybrid recursive approach)
		bool result = false;
		for (int i = 0; i < Count; i++)
		{
			var evaluationResults = JsonSchema.Evaluate2(schema, instance);
			result = evaluationResults.IsValid;
		}
		
		return result;
	}

	//[Benchmark]
	public bool NewSchemaValidation3()
	{
		// Parse and build the schema once
		using var schemaDoc = JsonDocument.Parse(_personSchemaText);
		var schema = JsonSchema.Build(schemaDoc.RootElement);
		
		// Parse the instance once
		using var instanceDoc = JsonDocument.Parse(_personInstanceText);
		var instance = instanceDoc.RootElement;
		
		// Run validation Count times using Evaluate3 (pure recursive approach)
		bool result = false;
		for (int i = 0; i < Count; i++)
		{
			var evaluationResults = JsonSchema.Evaluate3(schema, instance);
			result = evaluationResults.IsValid;
		}
		
		return result;
	}

	[Benchmark]
	public bool AstSchemaValidation()
	{
		// Parse and build the AST once
		using var schemaDoc = JsonDocument.Parse(_personSchemaText);
		var astSchema = AstSchema.Build(schemaDoc.RootElement);
		
		// Parse the instance once
		using var instanceDoc = JsonDocument.Parse(_personInstanceText);
		var instance = instanceDoc.RootElement;
		
		// Run validation Count times using AST approach
		bool result = false;
		for (int i = 0; i < Count; i++)
		{
			var evaluationResults = AstSchema.Evaluate(astSchema, instance);
			result = evaluationResults.IsValid;
		}
		
		return result;
	}
} 