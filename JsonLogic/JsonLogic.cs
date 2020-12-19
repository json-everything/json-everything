using System.Text.Json;
using Json.Logic.Components;

namespace Json.Logic
{
	public static class JsonLogic
	{
		public static LogicComponent And(LogicComponent a, LogicComponent b) => new AndComponent(a, b);
		public static LogicComponent Or(LogicComponent a, LogicComponent b) => new OrComponent(a, b);

		public static LogicComponent StrictEquals(LogicComponent a, LogicComponent b) => new StrictEqualsComponent(a, b);

		public static LogicComponent Literal(JsonElement value) => new LiteralComponent(value);
		public static LogicComponent Literal(int value) => new LiteralComponent(value);
		public static LogicComponent Literal(long value) => new LiteralComponent(value);
		public static LogicComponent Literal(decimal value) => new LiteralComponent(value);
		public static LogicComponent Literal(float value) => new LiteralComponent(value);
		public static LogicComponent Literal(double value) => new LiteralComponent(value);
		public static LogicComponent Literal(string value) => new LiteralComponent(value);
		public static LogicComponent Literal(bool value) => new LiteralComponent(value);
		public static LogicComponent Variable(string path, LogicComponent defaultValue) => new VariableComponent(Literal(path), defaultValue);
	}
}