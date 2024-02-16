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

		string? idString;
		var id = obj.TryGetValue(IdKeyword.Name, out var idNode, out _) &&
		         idNode is JsonValue idValue && (idString = idValue.GetString()) is not null
			? new Uri(idString, UriKind.Absolute)
			: GenerateBaseUri();

		var context = new FunctionalEvaluationContext
		{
			RootSchema = obj,
			LocalSchema = obj,
			RootInstance = instance,
			LocalInstance = instance,
			SchemaRegistry = new(),
			CurrentUri = id
		};
		context.SchemaRegistry.RegisterUntyped(id, schema);

		var result = true;
		foreach (var handler in SchemaKeywordRegistry.KeywordHandlers)
		{
			result &= handler.Evaluate(context);
		}

		return result;
	}

	public static bool Evaluate(FunctionalEvaluationContext context)
	{
		bool? boolSchema;
		if (context.LocalSchema is JsonValue value && (boolSchema = value.GetBool()).HasValue)
			return boolSchema.Value;

		// shouldn't ever happen, but custom keywords might do dumb things
		// also catches null
		if (context.LocalSchema is not JsonObject)
			throw new ArgumentException("schema must be a boolean or object");

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
	public JsonNode LocalSchema { get; set; }
	public JsonNode? RootInstance { get; set; }
	public JsonNode? LocalInstance { get; set; }
	public Uri CurrentUri { get; set; }
	public SchemaRegistry SchemaRegistry { get; set; } // will probably move to options later
}
