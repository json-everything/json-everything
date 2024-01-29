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
		new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

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

	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "We pass an SerializeToNode an Options context with the JsonTypeInfos we will need.")]
	[UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "We pass an SerializeToNode an Options context with the JsonTypeInfos we will need.")]
	private static JsonNode? Sort(JsonNode? node)
	{
		if (node is not JsonObject obj) return node;

		var dict = new SortedDictionary<string, JsonNode?>(StringComparer.Ordinal);
		foreach (var kvp in obj)
		{
			dict[kvp.Key] = Sort(kvp.Value);
		}
		
		// Can't use the TypeInfo overload because JsonSerializerContext differs in nullability (https://github.com/dotnet/runtime/issues/97665)
		return JsonSerializer.SerializeToNode(dict, JsonESerializerContext.Default.Options);
	}
}