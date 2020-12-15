using System.Text.Json;
using Json.Logic.Components;
using Json.Path;

namespace Json.Logic
{
	public static class JsonLogic
	{
		public static ILogicComponent And(ILogicComponent a, ILogicComponent b) => new AndComponent(a, b);
		public static ILogicComponent Or(ILogicComponent a, ILogicComponent b) => new OrComponent(a, b);

		public static ILogicComponent StrictEquals(ILogicComponent a, ILogicComponent b) => new StrictEqualsComponent(a, b);

		public static ILogicComponent Literal(JsonElement value) => new LiteralComponent(value);
		public static ILogicComponent Literal(int value) => new LiteralComponent(value);
		public static ILogicComponent Literal(long value) => new LiteralComponent(value);
		public static ILogicComponent Literal(decimal value) => new LiteralComponent(value);
		public static ILogicComponent Literal(float value) => new LiteralComponent(value);
		public static ILogicComponent Literal(double value) => new LiteralComponent(value);
		public static ILogicComponent Literal(string value) => new LiteralComponent(value);
		public static ILogicComponent Literal(bool value) => new LiteralComponent(value);
		public static ILogicComponent Variable(JsonPath path, ILogicComponent defaultValue) => new VariableComponent(path, defaultValue);
		public static ILogicComponent Variable(string path, ILogicComponent defaultValue) => new VariableComponent(JsonPath.Parse(path), defaultValue);
	}
}