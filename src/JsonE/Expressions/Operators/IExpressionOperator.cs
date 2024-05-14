namespace Json.JsonE.Expressions.Operators;

internal interface IExpressionOperator
{
	int Precedence { get; }
}