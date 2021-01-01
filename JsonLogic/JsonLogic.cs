using System.Text.Json;
using Json.Logic.Rules;

namespace Json.Logic
{
	public static class JsonLogic
	{
		public static Rule And(Rule a, params Rule[] more) => new AndRule(a, more);
		public static Rule Or(Rule a, params Rule[] more) => new OrRule(a, more);
		public static Rule Not(Rule value) => new NotRule(value);
		public static Rule BoolCast(Rule value) => new BooleanCastRule(value);

		public static Rule If(params Rule[] components) => new IfRule(components);

		public static Rule Add(Rule a, params Rule[] more) => new AddRule(a, more);
		public static Rule Subtract(Rule a, params Rule[] more) => new SubtractRule(a, more);
		public static Rule Multiply(Rule a, params Rule[] more) => new MultiplyRule(a, more);
		public static Rule Divide(Rule a, Rule b) => new DivideRule(a, b);
		public static Rule Modulus(Rule a, Rule b) => new ModRule(a, b);

		public static Rule Max(Rule a, params Rule[] more) => new MaxRule(a, more);
		public static Rule Min(Rule a, params Rule[] more) => new MinRule(a, more);

		public static Rule StrictEquals(Rule a, Rule b) => new StrictEqualsRule(a, b);
		public static Rule StrictNotEquals(Rule a, Rule b) => new StrictNotEqualsRule(a, b);
		public static Rule LooseEquals(Rule a, Rule b) => new LooseEqualsRule(a, b);
		public static Rule LooseNotEquals(Rule a, Rule b) => new LooseNotEqualsRule(a, b);
		public static Rule LessThan(Rule a, Rule b) => new LessThanRule(a, b);
		public static Rule BetweenExclusive(Rule a, Rule b, Rule c) => new LessThanRule(a, b, c);
		public static Rule LessThanOrEqual(Rule a, Rule b) => new LessThanEqualRule(a, b);
		public static Rule BetweenInclusive(Rule a, Rule b, Rule c) => new LessThanEqualRule(a, b, c);
		public static Rule MoreThan(Rule a, Rule b) => new MoreThanRule(a, b);
		public static Rule MoreThanOrEqual(Rule a, Rule b) => new MoreThanEqualRule(a, b);

		public static Rule Cat(Rule a, params Rule[] more) => new CatRule(a, more);
		public static Rule Substr(Rule input, Rule start) => new SubstrRule(input, start);
		public static Rule Substr(Rule input, Rule start, Rule count) => new SubstrRule(input, start, count);

		public static Rule All(Rule input, Rule rule) => new AllRule(input, rule);
		public static Rule Some(Rule input, Rule rule) => new SomeRule(input, rule);
		public static Rule None(Rule input, Rule rule) => new NoneRule(input, rule);
		public static Rule Missing(params Rule[] components) => new MissingRule(components);
		public static Rule MissingSome(Rule requiredCount, Rule components) => new MissingSomeRule(requiredCount, components);
		public static Rule In(Rule test, Rule source) => new InRule(test, source);

		public static Rule Map(Rule input, Rule rule) => new MapRule(input, rule);
		public static Rule Reduce(Rule input, Rule rule, Rule initial) => new ReduceRule(input, rule, initial);
		public static Rule Filter(Rule input, Rule rule) => new FilterRule(input, rule);
		public static Rule Merge(params Rule[] items) => new MergeRule(items);

		public static Rule Literal(JsonElement value) => new LiteralRule(value);
		public static Rule Literal(int value) => new LiteralRule(value);
		public static Rule Literal(long value) => new LiteralRule(value);
		public static Rule Literal(decimal value) => new LiteralRule(value);
		public static Rule Literal(float value) => new LiteralRule(value);
		public static Rule Literal(double value) => new LiteralRule(value);
		public static Rule Literal(string value) => new LiteralRule(value);
		public static Rule Literal(bool value) => new LiteralRule(value);
		public static Rule Variable(string path, Rule defaultValue) => new VariableRule(Literal(path), defaultValue);

		public static Rule Log(Rule log) => new LogRule(log);

	}
}