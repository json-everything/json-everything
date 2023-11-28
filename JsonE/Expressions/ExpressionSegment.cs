using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Expressions;

internal class ExpressionSegment : IContextAccessorSegment
{
	private readonly ExpressionNode _expression;

	public ExpressionSegment(ExpressionNode expression)
	{
		_expression = expression;
	}

	public bool TryFind(JsonNode? contextValue, EvaluationContext fullContext, out JsonNode? value)
	{
		if (_expression.Evaluate(fullContext) is JsonValue evaluated)
		{
			if (evaluated.TryGetValue(out string? prop))
				return PropertySegment.TryFind(prop, true, contextValue, out value);
			var number = evaluated.GetNumber();
			if (number.HasValue)
			{
				var index = (int)number;
				if (number == index)
					return IndexSegment.TryFind(index, contextValue, out value);
			}
		}

		throw new InterpreterException("object keys must be strings");
	}
}