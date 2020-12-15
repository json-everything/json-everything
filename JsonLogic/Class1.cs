using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.More;

namespace Json.Logic
{
	public interface IOperator
	{
		List<IOperand> Operands { get; }

		JsonElement Apply(JsonElement data);
	}

	public interface IOperand
	{
		JsonElement Apply(JsonElement data);
	}

	public class EqualsOperator : IOperator
	{
		public List<IOperand> Operands { get; }

		public EqualsOperator(IOperand a, IOperand b)
		{
			Operands = new List<IOperand> {a, b};
		}

		public JsonElement Apply(JsonElement data)
		{
			if (Operands.Count != 2)
				throw new JsonLogicException("`==` operator requires exactly two arguments.");

			var equals = Operands[0].Apply(data).IsEquivalentTo(Operands[1].Apply(data));

			return equals.AsJsonElement();
		}
	}

	public class AndOperator : IOperator
	{
		public List<IOperand> Operands { get; }

		public AndOperator(IOperand a, IOperand b)
		{
			Operands = new List<IOperand> { a, b };
		}

		public JsonElement Apply(JsonElement data)
		{
			if (Operands.Count != 2)
				throw new JsonLogicException("`and` operator requires exactly two arguments.");

			return (Operands[0].Apply(data).ValueKind == JsonValueKind.True &&
			        Operands[1].Apply(data).ValueKind == JsonValueKind.True).AsJsonElement();
		}
	}

	public class JsonLogicException : Exception
	{
		public JsonLogicException(string message)
			: base(message)
		{
		}
	}

	public class VariableOperator : IOperator
	{
		public List<IOperand> Operands { get; }

		public JsonElement Apply(JsonElement data)
		{
			throw new NotImplementedException();
		}
	}

	public class Variable : IOperand
	{
		public JsonElement Apply(JsonElement data)
		{
			throw new NotImplementedException();
		}
	}

	public class Literal : IOperand
	{
		private readonly JsonElement _value;

		public Literal(JsonElement value)
		{
			_value = value.Clone();
		}

		public Literal(int value)
		{
			_value = value.AsJsonElement();
		}

		public Literal(string value)
		{
			_value = value.AsJsonElement();
		}

		public JsonElement Apply(JsonElement data)
		{
			return _value;
		}
	}
}
