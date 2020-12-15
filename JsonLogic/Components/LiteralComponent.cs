using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	internal class LiteralComponent : ILogicComponent
	{
		private readonly JsonElement _value;

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
			_value = value.AsJsonElement();
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

		public JsonElement Apply(JsonElement data)
		{
			return _value;
		}
	}
}