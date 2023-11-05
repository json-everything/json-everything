using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Functions;

internal class TypeOfFunction : FunctionDefinition
{
	public override string Name => "typeof";
	public override FunctionValueType[] ParameterTypes { get; } = { FunctionValueType.Any };
	public override FunctionValueType ReturnType => FunctionValueType.String;

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
				break;
		}

		throw new InterpreterException("this shouldn't happen");
	}
}