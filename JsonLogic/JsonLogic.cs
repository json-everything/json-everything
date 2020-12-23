using System.Text.Json;
using Json.Logic.Rules;

namespace Json.Logic
{
	public static class JsonLogic
	{
		public static Rule And(Rule a, Rule b) => new AndRule(a, b);
		public static Rule Or(Rule a, Rule b) => new OrRule(a, b);

		public static Rule StrictEquals(Rule a, Rule b) => new StrictEqualsRule(a, b);

		public static Rule Literal(JsonElement value) => new LiteralRule(value);
		public static Rule Literal(int value) => new LiteralRule(value);
		public static Rule Literal(long value) => new LiteralRule(value);
		public static Rule Literal(decimal value) => new LiteralRule(value);
		public static Rule Literal(float value) => new LiteralRule(value);
		public static Rule Literal(double value) => new LiteralRule(value);
		public static Rule Literal(string value) => new LiteralRule(value);
		public static Rule Literal(bool value) => new LiteralRule(value);
		public static Rule Variable(string path, Rule defaultValue) => new VariableRule(Literal(path), defaultValue);
	}
}