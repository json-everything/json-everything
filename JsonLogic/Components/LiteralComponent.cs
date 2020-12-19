using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	[Operator("")]
	internal class LiteralComponent : LogicComponent
	{
		private readonly JsonElement _value;

		public static readonly LiteralComponent Null = new LiteralComponent(null);

		public LiteralComponent(JsonElement value)
		{
			_value = value.Clone();
		}

		public LiteralComponent(int value)
		{
			_value = value.AsJsonElement();
		}

		public LiteralComponent(string value)
		{
			_value = value?.AsJsonElement() ?? JsonDocument.Parse("null").RootElement;
		}

		public LiteralComponent(bool value)
		{
			_value = value.AsJsonElement();
		}

		public LiteralComponent(long value)
		{
			_value = value.AsJsonElement();
		}

		public LiteralComponent(decimal value)
		{
			_value = value.AsJsonElement();
		}

		public LiteralComponent(float value)
		{
			_value = value.AsJsonElement();
		}

		public LiteralComponent(double value)
		{
			_value = value.AsJsonElement();
		}

		public override JsonElement Apply(JsonElement data)
		{
			return _value;
		}
	}
}