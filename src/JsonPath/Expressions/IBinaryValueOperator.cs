namespace Json.Path.Expressions;

internal interface IBinaryValueOperator : IExpressionOperator
{
	PathValue? Evaluate(PathValue? left, PathValue? right);
}

internal interface IExpressionOperator
{
	int Precedence { get; }
}