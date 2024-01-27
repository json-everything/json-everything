using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;

var schema = JsonSchema.FromText("""
                                 {
                                   "$schema": "https://json-schema.org/draft/2020-12/schema",
                                   "type": "object",
                                   "properties": {
                                     "foo": { "type": "integer" }
                                   }
                                 }
                                 """);

var instance = JsonNode.Parse("{\"foo\": 1}");

var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

var serializerOptions = new JsonSerializerOptions
{
	WriteIndented = true,
	Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};
serializerOptions.TypeInfoResolverChain.Add(JsonSchema.TypeInfoResolver);

Console.WriteLine("Schema:");
Console.WriteLine(JsonSerializer.Serialize(schema, serializerOptions.GetTypeInfo(typeof(JsonSchema))));

Console.WriteLine("Result:");
Console.WriteLine(JsonSerializer.Serialize(result, serializerOptions.GetTypeInfo(typeof(EvaluationResults))));
