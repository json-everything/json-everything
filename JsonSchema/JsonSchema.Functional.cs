using System;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema;

public partial class JsonSchema
{
	public static bool Evaluate(JsonNode? schema, JsonNode? instance)
	{
		bool? boolSchema;
		if (schema is JsonValue value && (boolSchema = value.GetBool()).HasValue)
			return boolSchema.Value;

		if (schema is not JsonObject obj)
			throw new ArgumentException("schema must be a boolean or object");

		var context = new FunctionalEvaluationContext
		{
			RootSchema = obj,
			LocalSchema = obj,
			RootInstance = instance,
			LocalInstance = instance
		};

		var result = true;
		foreach (var handler in SchemaKeywordRegistry.KeywordHandlers)
		{
			result &= handler.Evaluate(context);
		}

		return result;
	}
}

public interface IKeywordHandler
{
	bool Evaluate(FunctionalEvaluationContext context);
}

public struct FunctionalEvaluationContext
{
	public JsonObject RootSchema { get; set; }
	public JsonObject LocalSchema { get; set; }
	public JsonNode? RootInstance { get; set; }
	public JsonNode? LocalInstance { get; set; }
}
