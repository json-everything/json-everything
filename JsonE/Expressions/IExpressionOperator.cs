namespace Json.JsonE.Expressions;

internal interface IExpressionOperator
{
	int Precedence { get; }
}