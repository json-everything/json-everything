using System.Text.Json;
using Json.More;

namespace Json.Logic.Rules
{
	[Operator("")]
	internal class LiteralRule : Rule
	{
		private readonly JsonElement _value;

		public static readonly LiteralRule Null = new LiteralRule(null);

		public LiteralRule(JsonElement value)
		{
			_value = value.Clone();
		}

		public LiteralRule(int value)
		{
			_value = value.AsJsonElement();
		}

		public LiteralRule(string? value)
		{
			_value = value?.AsJsonElement() ?? JsonDocument.Parse("null").RootElement;
		}

		public LiteralRule(bool value)
		{
			_value = value.AsJsonElement();
		}

		public LiteralRule(long value)
		{
			_value = value.AsJsonElement();
		}

		public LiteralRule(decimal value)
		{
			_value = value.AsJsonElement();
		}

		public LiteralRule(float value)
		{
			_value = value.AsJsonElement();
		}

		public LiteralRule(double value)
		{
			_value = value.AsJsonElement();
		}

		public override JsonElement Apply(JsonElement data)
		{
			return _value;
		}
	}
}