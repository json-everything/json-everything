using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Expressions.Functions;

internal class TypeOfFunction : FunctionDefinition
{
	// I explicitly want this for consistency and code-doc
	// ReSharper disable once UnusedMember.Local
#pragma warning disable IDE0051 // Remove unused private members
	private const string _name = "typeof";
#pragma warning restore IDE0051 // Remove unused private members

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		switch (arguments[0])
		{
			case null:
				return "null";
			case JsonObject:
				return "object";
			case JsonArray:
				return "array";
			case JsonValue val:
				if (val.GetNumber().HasValue) return "number";
				if (val.TryGetValue<string>(out _)) return "string";
				if (val.TryGetValue<bool>(out _)) return "boolean";
				if (val.TryGetValue<FunctionDefinition>(out _)) return "function";
				break;
		}

		throw new InterpreterException("this shouldn't happen");
	}
}