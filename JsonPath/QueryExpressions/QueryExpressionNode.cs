using System;
using System.Text.Json;

namespace Json.Path.QueryExpressions
{
	internal class QueryExpressionNode
	{
		private QueryExpressionType? _outputKind;
		private JsonElement? _value;

		public QueryExpressionNode Left { get; }
		public IQueryExpressionOperator Operator { get; }
		public QueryExpressionNode Right { get; }

		public QueryExpressionType OutputType => _outputKind ??= Operator?.GetOutputType(Left, Right) ?? GetValueType();
		public JsonElement Value => OutputType == QueryExpressionType.Invalid
			? default
			: _value ??= Operator.Evaluate(Left, Right);

		public QueryExpressionNode(JsonElement value)
		{
			_value = value;
		}

		public QueryExpressionNode(QueryExpressionNode left, IQueryExpressionOperator op, QueryExpressionNode right)
		{
			Left = left ?? throw new ArgumentNullException(nameof(left));
			Operator = op ?? throw new ArgumentNullException(nameof(op));
			Right = right;
		}

		private QueryExpressionType GetValueType()
		{
			// ReSharper disable once PossibleInvalidOperationException
			return _value.Value.ValueKind switch
			{
				JsonValueKind.Undefined => QueryExpressionType.Invalid,
				JsonValueKind.Object => QueryExpressionType.Invalid,
				JsonValueKind.Array => QueryExpressionType.Invalid,
				JsonValueKind.String => QueryExpressionType.String,
				JsonValueKind.Number => QueryExpressionType.Number,
				JsonValueKind.True => QueryExpressionType.Boolean,
				JsonValueKind.False => QueryExpressionType.Boolean,
				JsonValueKind.Null => QueryExpressionType.Invalid,
				_ => throw new ArgumentOutOfRangeException()
			};
		}

		public override string ToString()
		{
			return Operator?.ToString(Left, Right) ?? _value.ToString();
		}
	}
}
