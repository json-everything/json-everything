using System.Linq;
using System.Text.Json.Nodes;
using Json.JsonE.Operators;
using Json.More;

namespace Json.JsonE.Functions;

internal class JoinFunction : FunctionDefinition
{
	public override string Name => "join";
	public override FunctionValueType[] ParameterTypes { get; } = { FunctionValueType.Array, FunctionValueType.String | FunctionValueType.Number };
	public override FunctionValueType ReturnType => FunctionValueType.String;

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		if (arguments[0] is not JsonArray arr)
			throw new TemplateException(CommonErrors.IncorrectArgType(Name));

		if (arguments[1] is not JsonValue v || !v.TryGetValue(out string? separator))
			throw new TemplateException(CommonErrors.IncorrectArgType(Name));

		var parts = arr.Select(x =>
		{
			if (x is not JsonValue val)
				throw new TemplateException(CommonErrors.IncorrectArgType(Name));

			var num = val.GetNumber();
			if (num.HasValue) return num.ToString();

			if (val.TryGetValue(out string? str)) return str;

			if (val.TryGetValue(out bool b)) return b.ToString().ToLowerInvariant();

			throw new TemplateException(CommonErrors.IncorrectArgType(Name));
		});

		return string.Join(separator, parts);
	}
}