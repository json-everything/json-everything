using System.Text;

namespace Json.JsonE.Expressions;

internal interface IExpression
{
	bool Evaluate(EvaluationContext context);
	void BuildString(StringBuilder builder);
}