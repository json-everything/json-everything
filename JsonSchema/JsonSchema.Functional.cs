﻿using System;
using System.Collections.Generic;
using System.Linq;
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
			throw new ArgumentException("Schema must be a boolean or object");

		string? idString;
		var id = obj.TryGetValue(IdKeyword.Name, out var idNode, out _) &&
		         idNode is JsonValue idValue && (idString = idValue.GetString()) is not null
			? new Uri(idString, UriKind.Absolute)
			: GenerateBaseUri();

		var context = new FunctionalEvaluationContext
		{
			LocalSchema = obj,
			LocalInstance = instance,
			CurrentUri = id,
			SchemaRegistry = new(),
			Annotations = new()
		};
		context.SchemaRegistry.RegisterUntyped(id, schema);

		var result = true;
		foreach (var handler in SchemaKeywordRegistry.KeywordHandlers.OrderBy(x => ((IJsonSchemaKeyword)x).Priority()))
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
		if (context.LocalSchema is not JsonObject obj)
			throw new ArgumentException("Schema must be a boolean or object");

		string? idString;
		var id = obj.TryGetValue(IdKeyword.Name, out var idNode, out _) &&
		         idNode is JsonValue idValue && (idString = idValue.GetString()) is not null
			? new Uri(context.CurrentUri, idString)
			: null;

		if (id is not null) context.CurrentUri = id;
		context.Annotations = new();

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
	public JsonNode LocalSchema { get; set; }
	public JsonNode? LocalInstance { get; set; }
	public Uri CurrentUri { get; set; }
	public SchemaRegistry SchemaRegistry { get; set; } // will probably move to options later
	public Dictionary<string, JsonNode> Annotations { get; set; }
}
