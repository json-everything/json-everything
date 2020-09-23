using System;
using System.Text.Json;
using Json.More;

namespace Json.Path.QueryExpressions
{
	internal class QueryExpressionNode
	{
		private readonly JsonPath _path;
		private QueryExpressionType? _outputType;
		private JsonElement? _value;

		public QueryExpressionNode Left { get; }
		public IQueryExpressionOperator Operator { get; }
		public QueryExpressionNode Right { get; private set; }

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
				// don't set _value; need to always eval
				return result.Matches.Count == 1
					? result.Matches[0].Value
					: default;
			}

			if (OutputType == QueryExpressionType.Invalid) return default;
			
			var value = Operator.Evaluate(Left, Right, element);
			if (OutputType != QueryExpressionType.InstanceDependent)
				_value = value;
			return value;
		}

		public void InsertRight(IQueryExpressionOperator op, QueryExpressionNode newRight)
		{
			Right = new QueryExpressionNode(Right, op, newRight);
		}

		public static bool TryParseSingle(ReadOnlySpan<char> span, ref int i, out QueryExpressionNode node)
		{
			if (JsonPath.TryParse(span, ref i, true, out var path))
			{
				node = new QueryExpressionNode(path);
				return true;
			}

			// TODO: this should really be extracting a JsonElement,
			// but I don't know how to parse that from a string with trailing content
			if (span.TryGetInt(ref i, out var value))
			{
				node = new QueryExpressionNode(value.AsJsonElement());
				return true;
			}

			node = null;
			return false;
		}

		private QueryExpressionType GetOutputType()
		{
			if (_value.HasValue) return GetValueType();
			if (_path != null) return QueryExpressionType.InstanceDependent;

			if (Left.OutputType == QueryExpressionType.Invalid ||
			    Right?.OutputType == QueryExpressionType.Invalid)
				return QueryExpressionType.Invalid;

			// TODO: this might be optimizable depending on the operation
			if (Left.OutputType == QueryExpressionType.InstanceDependent ||
			    Right?.OutputType == QueryExpressionType.InstanceDependent)
				return QueryExpressionType.InstanceDependent;

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
			return Operator?.ToString(Left, Right) ?? _value?.ToJsonString() ?? _path.ToString();
		}
	}
}
