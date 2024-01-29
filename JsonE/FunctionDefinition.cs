using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.JsonE.Expressions.Functions;

namespace Json.JsonE;

/// <summary>
/// Serves as a base type for JSON-e functions.  Use <see cref="JsonFunction"/> to create custom functions.
/// </summary>
public abstract class FunctionDefinition
{
	internal abstract JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context);

	/// <summary>
	/// Implicitly converts a <see cref="FunctionDefinition"/> into a <see cref="JsonNode"/>.
	/// </summary>
	/// <param name="func">The function.</param>
	public static implicit operator JsonNode?(FunctionDefinition func)
	{
		return JsonValue.Create(func, JsonESerializerContext.Default.FunctionDefinition);
	}
}
