using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Operators;

internal class JsonOperator : IOperator
{
	private static readonly JsonSerializerOptions _serializerOptions =
		new(JsonESerializerContext.Default.Options)
		{
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};

	public const string Name = "$json";

	public JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		var obj = template!.AsObject();
		obj.VerifyNoUndefinedProperties(Name);
	
		var value = obj[Name];

		var evaluated = Sort(JsonE.Evaluate(value, context));
		evaluated.ValidateNotReturningFunction();

		return evaluated.AsJsonString(_serializerOptions);
	}

	private static JsonNode? Sort(JsonNode? node)
	{
		if (node is not JsonObject obj) return node;

		var dict = new SortedDictionary<string, JsonNode?>(StringComparer.Ordinal);
		foreach (var kvp in obj)
		{
			dict[kvp.Key] = Sort(kvp.Value);
		}
		
		return JsonSerializer.SerializeToNode(dict!, JsonESerializerContext.Default.SortedDictionaryStringJsonNode);
	}
}