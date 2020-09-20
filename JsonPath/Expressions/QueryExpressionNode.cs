using System.Text.Json;

namespace Json.Path.Expressions
{
	internal abstract class QueryExpressionNode
	{
		private QueryExpressionType? _outputKind;
		private JsonElement? _value;

		public QueryExpressionNode Left { get; }
		public IQueryExpressionOperator Operator { get; }
		public QueryExpressionNode Right { get; }

		public QueryExpressionType OutputKind => _outputKind ??= Operator.GetOutputKind(Left, Right);
		public JsonElement Value => _value ??= Operator.Evaluate(Left, Right);

		protected QueryExpressionNode(JsonElement value)
		{
			_value = value;
		}

		protected QueryExpressionNode(QueryExpressionNode left, IQueryExpressionOperator op, QueryExpressionNode right)
		{
			Left = left;
			Operator = op;
			Right = right;
		}
	}
}
