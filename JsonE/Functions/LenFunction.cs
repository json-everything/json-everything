using System.Globalization;
using System.Text.Json.Nodes;
using Json.JsonE.Expressions;
using Json.JsonE.Operators;

namespace Json.JsonE.Functions;

internal class LenFunction : FunctionDefinition
{
	public override string Name => "len";
	public override FunctionValueType[] ParameterTypes { get; } = { FunctionValueType.Array | FunctionValueType.String };
	public override FunctionValueType ReturnType => FunctionValueType.Number;

	internal override JsonNode? Invoke(JsonNode?[] arguments)
	{
		var arg = arguments[0];

		if (arg is JsonArray arr) return arr.Count;
		if (arg is JsonValue val && val.TryGetValue(out string? str)) return new StringInfo(str).LengthInTextElements;

		throw new InterpreterException(CommonErrors.IncorrectValueTypeFunction(Name));
	}
}