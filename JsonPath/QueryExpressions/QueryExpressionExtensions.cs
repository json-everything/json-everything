namespace Json.Path.QueryExpressions
{
	internal static class QueryExpressionExtensions
	{
		public static string MaybeAddParentheses(this QueryExpressionNode node, int operationOrder, bool overrideIfSame = false)
		{
			var asString = node.ToString();
			if (operationOrder < node.Operator?.OrderOfOperation)
				asString = $"({asString})";
			if (overrideIfSame && operationOrder == node.Operator?.OrderOfOperation)
				asString = $"({asString})";

			return asString;
		}
	}
}