using System;
using System.Text.Json;

namespace Json.Path.QueryExpressions
{
	internal class QueryExpressionNode
	{
		private readonly JsonPath _path;
		private QueryExpressionType? _outputType;
		private JsonElement? _value;

		public QueryExpressionNode Left { get; }
		public IQueryExpressionOperator Operator { get; }
		public QueryExpressionNode Right { get; }

		public QueryExpressionType OutputType => _outputType ??= GetOutputType();

		public QueryExpressionNode(JsonElement value)
		{
			_value = value;
		}

		public QueryExpressionNode(JsonPath path)
		{
			_path = path;
			_outputType = QueryExpressionType.InstanceDependent;
		}

		public QueryExpressionNode(QueryExpressionNode left, IQueryExpressionOperator op, QueryExpressionNode right)
		{
			Left = left ?? throw new ArgumentNullException(nameof(left));
			Operator = op ?? throw new ArgumentNullException(nameof(op));
			Right = right;
		}

		public JsonElement Evaluate(JsonElement element)
		{
			if (_value.HasValue) return _value.Value;

			if (_path != null)
			{
				var result = _path.Evaluate(element);
				// don't set value; need to always eval
				return result.Matches.Count == 1
					? result.Matches[0].Value
					: default;
			}

			return OutputType == QueryExpressionType.Invalid
				? default
				: _value ??= Operator.Evaluate(Left, Right, element);
		}

		private QueryExpressionType GetOutputType()
		{
			if (_value.HasValue) return GetValueType();
			if (_path != null) return QueryExpressionType.InstanceDependent;

			if (Left.OutputType == QueryExpressionType.Invalid ||
			    Right.OutputType == QueryExpressionType.Invalid)
				return QueryExpressionType.Invalid;

			// this might be optimizable depending on the operation
			if (Left.OutputType == QueryExpressionType.InstanceDependent ||
			    Right.OutputType == QueryExpressionType.InstanceDependent)
				return QueryExpressionType.InstanceDependent;

			if (_path != null) return QueryExpressionType.InstanceDependent;

			return Operator?.GetOutputType(Left, Right) ?? GetValueType();
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
