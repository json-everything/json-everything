﻿using System;
using System.Linq;
using System.Text.Json.Nodes;
using Json.Logic.Rules;
using Json.More;

namespace Json.Logic;

/// <summary>
/// Provides factory methods to create operations.
/// </summary>
public static class JsonLogic
{
	/// <summary>
	/// Applies a rule encoded into a <see cref="JsonNode"/> to some data.
	/// </summary>
	/// <param name="rule">The rule to apply.</param>
	/// <param name="context">The context data.</param>
	/// <returns>The result.</returns>
	public static JsonNode? Apply(JsonNode? rule, JsonNode? context = null)
	{
		var evalContext = new EvaluationContext(context);
		return Apply(rule, evalContext);
	}

	/// <summary>
	/// Applies a nested rule encoded into a <see cref="JsonNode"/> to a context.
	/// This is to be called from within an <see cref="IRule"/> handler.
	/// </summary>
	/// <param name="rule">The rule to apply.</param>
	/// <param name="context">The context data.</param>
	/// <returns>The result.</returns>
	public static JsonNode? Apply(JsonNode? rule, EvaluationContext context)
	{
		switch (rule)
		{
			case JsonObject { Count: 1 } obj:
				var (key, value) = obj.Single();
				var handler = RuleRegistry.GetHandler(key);

				return handler is null ? rule : handler.Apply(value, context);
			case JsonArray array:
				return array.Select(x => Apply(x, context)).ToJsonArray();
		}

		return rule;
	}

	/// <summary>
	/// Creates an `and` rule.
	/// </summary>
	/// <param name="a">The first rule.</param>
	/// <param name="more">Subsequent rules.</param>
	/// <returns>An `and` rule.</returns>
	public static Rule And(Rule a, params Rule[] more) => new AndRule(a, more);

#if NET9_0_OR_GREATER
	/// <summary>
	/// Creates an `and` rule.
	/// </summary>
	/// <param name="a">The first rule.</param>
	/// <param name="more">Subsequent rules.</param>
	/// <returns>An `and` rule.</returns>
	public static Rule And(Rule a, params ReadOnlySpan<Rule> more) => new AndRule(a, more);
#endif

	/// <summary>
	/// Creates an `or` rule.
	/// </summary>
	/// <param name="a">The first rule.</param>
	/// <param name="more">Subsequent rules.</param>
	/// <returns>An `or` rule.</returns>
	public static Rule Or(Rule a, params Rule[] more) => new OrRule(a, more);

#if NET9_0_OR_GREATER
	/// <summary>
	/// Creates an `or` rule.
	/// </summary>
	/// <param name="a">The first rule.</param>
	/// <param name="more">Subsequent rules.</param>
	/// <returns>An `or` rule.</returns>
	public static Rule Or(Rule a, params ReadOnlySpan<Rule> more) => new OrRule(a, more);
#endif

	/// <summary>
	/// Creates a `!` ("not") rule.
	/// </summary>
	/// <param name="value">The rule to negate.</param>
	/// <returns>A `!` rule.</returns>
	public static Rule Not(Rule value) => new NotRule(value);
	
	/// <summary>
	/// Creates a `!!` ("boolean cast") rule.
	/// </summary>
	/// <param name="value">The rule to negate.</param>
	/// <returns>A `!!` rule.</returns>
	public static Rule BoolCast(Rule value) => new BooleanCastRule(value);

	/// <summary>
	/// Creates an `if` rule.
	/// </summary>
	/// <param name="components">The rule chain to process.</param>
	/// <returns>An `if` rule.</returns>
	public static Rule If(params Rule[] components) => new IfRule(components);

	/// <summary>
	/// Creates a `+` ("add") rule.
	/// </summary>
	/// <param name="a">The first rule.</param>
	/// <param name="more">Subsequent rules.</param>
	/// <returns>A `+` rule.</returns>
	public static Rule Add(Rule a, params Rule[] more) => new AddRule(a, more);

#if NET9_0_OR_GREATER
	/// <summary>
	/// Creates a `+` ("add") rule.
	/// </summary>
	/// <param name="a">The first rule.</param>
	/// <param name="more">Subsequent rules.</param>
	/// <returns>A `+` rule.</returns>
	public static Rule Add(Rule a, params ReadOnlySpan<Rule> more) => new AddRule(a, more);
#endif

	/// <summary>
	/// Creates a `-` ("subtract") rule.
	/// </summary>
	/// <param name="a">The first rule.</param>
	/// <param name="more">Subsequent rules.</param>
	/// <returns>A `-` rule.</returns>
	public static Rule Subtract(Rule a, params Rule[] more) => new SubtractRule(a, more);

#if NET9_0_OR_GREATER
	/// <summary>
	/// Creates a `-` ("subtract") rule.
	/// </summary>
	/// <param name="a">The first rule.</param>
	/// <param name="more">Subsequent rules.</param>
	/// <returns>A `-` rule.</returns>
	public static Rule Subtract(Rule a, params ReadOnlySpan<Rule> more) => new SubtractRule(a, more);
#endif

	/// <summary>
	/// Creates a `*` ("multiply") rule.
	/// </summary>
	/// <param name="a">The first rule.</param>
	/// <param name="more">Subsequent rules.</param>
	/// <returns>A `*` rule.</returns>
	public static Rule Multiply(Rule a, params Rule[] more) => new MultiplyRule(a, more);

#if NET9_0_OR_GREATER
	/// <summary>
	/// Creates a `*` ("multiply") rule.
	/// </summary>
	/// <param name="a">The first rule.</param>
	/// <param name="more">Subsequent rules.</param>
	/// <returns>A `*` rule.</returns>
	public static Rule Multiply(Rule a, params ReadOnlySpan<Rule> more) => new MultiplyRule(a, more);
#endif

	/// <summary>
	/// Creates a `/` ("divide") rule.
	/// </summary>
	/// <param name="a">The first rule.</param>
	/// <param name="b"></param>
	/// <returns>A `/` rule.</returns>
	public static Rule Divide(Rule a, Rule b) => new DivideRule(a, b);
	
	/// <summary>
	/// Creates a `%` ("modulus") rule.
	/// </summary>
	/// <param name="a">The first rule.</param>
	/// <param name="b"></param>
	/// <returns>A `%` rule.</returns>
	public static Rule Modulus(Rule a, Rule b) => new ModRule(a, b);

	/// <summary>
	/// Creates a `max` rule.
	/// </summary>
	/// <param name="a">The first rule.</param>
	/// <param name="more">Subsequent rules.</param>
	/// <returns>A `max` rule.</returns>
	public static Rule Max(Rule a, params Rule[] more) => new MaxRule(a, more);

#if NET9_0_OR_GREATER
	/// <summary>
	/// Creates a `max` rule.
	/// </summary>
	/// <param name="a">The first rule.</param>
	/// <param name="more">Subsequent rules.</param>
	/// <returns>A `max` rule.</returns>
	public static Rule Max(Rule a, params ReadOnlySpan<Rule> more) => new MaxRule(a, more);
#endif

	/// <summary>
	/// Creates a `min` rule.
	/// </summary>
	/// <param name="a">The first rule.</param>
	/// <param name="more">Subsequent rules.</param>
	/// <returns>A `min` rule.</returns>
	public static Rule Min(Rule a, params Rule[] more) => new MinRule(a, more);

#if NET9_0_OR_GREATER
	/// <summary>
	/// Creates a `min` rule.
	/// </summary>
	/// <param name="a">The first rule.</param>
	/// <param name="more">Subsequent rules.</param>
	/// <returns>A `min` rule.</returns>
	public static Rule Min(Rule a, params ReadOnlySpan<Rule> more) => new MinRule(a, more);
#endif

	/// <summary>
	/// Creates a `===` ("strict equal") rule.
	/// </summary>
	/// <param name="a">The first rule.</param>
	/// <param name="b">The second rule.</param>
	/// <returns>A `===` rule.</returns>
	public static Rule StrictEquals(Rule a, Rule b) => new StrictEqualsRule(a, b);
	
	/// <summary>
	/// Creates a `!==` ("strict not equal") rule.
	/// </summary>
	/// <param name="a">The first rule.</param>
	/// <param name="b">The second rule.</param>
	/// <returns>A `!==` rule.</returns>
	
	public static Rule StrictNotEquals(Rule a, Rule b) => new StrictNotEqualsRule(a, b);
	/// <summary>
	/// Creates a `==` ("loose equal") rule.
	/// </summary>
	/// <param name="a">The first rule.</param>
	/// <param name="b">The second rule.</param>
	/// <returns>A `==` rule.</returns>
	
	public static Rule LooseEquals(Rule a, Rule b) => new LooseEqualsRule(a, b);
	/// <summary>
	/// Creates a `!=` ("loose not equal") rule.
	/// </summary>
	/// <param name="a">The first rule.</param>
	/// <param name="b">The second rule.</param>
	/// <returns>A `!=` rule.</returns>
	public static Rule LooseNotEquals(Rule a, Rule b) => new LooseNotEqualsRule(a, b);
	/// <summary>
	/// Creates a `&lt;` ("less than") rule.
	/// </summary>
	/// <param name="a">The lower limit rule.</param>
	/// <param name="b">The upper limit rule.</param>
	/// <returns>A `&lt;` rule.</returns>
	public static Rule LessThan(Rule a, Rule b) => new LessThanRule(a, b);

	/// <summary>
	/// Creates a three-argument `&lt;` ("exclusive between") rule.
	/// </summary>
	/// <param name="a">The lower limit rule.</param>
	/// <param name="b">The rule.</param>
	/// <param name="c">The upper limit rule.</param>
	/// <returns>A `&lt;` rule.</returns>
	public static Rule BetweenExclusive(Rule a, Rule b, Rule c) => new LessThanRule(a, b, c);
	/// <summary>
	/// Creates a `&lt;=` ("less than or equal") rule.
	/// </summary>
	/// <param name="a">The lower limit rule.</param>
	/// <param name="b">The upper limit rule.</param>
	/// <returns>A `&lt;=` rule.</returns>
	public static Rule LessThanOrEqual(Rule a, Rule b) => new LessThanEqualRule(a, b);

	/// <summary>
	/// Creates a three-argument `&lt;=` ("inclusive between") rule.
	/// </summary>
	/// <param name="a">The lower limit rule.</param>
	/// <param name="b">The second rule.</param>
	/// <param name="c">The upper limit rule.</param>
	/// <returns>A `&lt;=` rule.</returns>
	public static Rule BetweenInclusive(Rule a, Rule b, Rule c) => new LessThanEqualRule(a, b, c);
	/// <summary>
	/// Creates a `&gt;` ("more than") rule.
	/// </summary>
	/// <param name="a">The upper limit rule.</param>
	/// <param name="b">The lower limit rule.</param>
	/// <returns>A `&gt;` rule.</returns>
	public static Rule MoreThan(Rule a, Rule b) => new MoreThanRule(a, b);
	/// <summary>
	/// Creates a `&gt;=` ("more than or equal") rule.
	/// </summary>
	/// <param name="a">The upper limit rule.</param>
	/// <param name="b">The lower limit rule.</param>
	/// <returns>A `&gt;` rule.</returns>
	public static Rule MoreThanOrEqual(Rule a, Rule b) => new MoreThanEqualRule(a, b);

	/// <summary>
	/// Creates a `cat` ("concatenation") rule.
	/// </summary>
	/// <param name="a">The first rule.</param>
	/// <param name="more">Subsequent rules.</param>
	/// <returns>A `cat` rule.</returns>
	public static Rule Cat(Rule a, params Rule[] more) => new CatRule(a, more);

#if NET9_0_OR_GREATER
	/// <summary>
	/// Creates a `cat` ("concatenation") rule.
	/// </summary>
	/// <param name="a">The first rule.</param>
	/// <param name="more">Subsequent rules.</param>
	/// <returns>A `cat` rule.</returns>
	public static Rule Cat(Rule a, params ReadOnlySpan<Rule> more) => new CatRule(a, more);
#endif

	/// <summary>
	/// Creates a `substr` ("concatenation") rule.
	/// </summary>
	/// <param name="input">The input rule.</param>
	/// <param name="start">The start rule.</param>
	/// <returns>A `substr` rule.</returns>
	public static Rule Substr(Rule input, Rule start) => new SubstrRule(input, start);
	
	/// <summary>
	/// Creates a `substr` ("concatenation") rule.
	/// </summary>
	/// <param name="input">The input rule.</param>
	/// <param name="start">The start rule.</param>
	/// <param name="count">The count rule.</param>
	/// <returns>A `substr` rule.</returns>
	public static Rule Substr(Rule input, Rule start, Rule count) => new SubstrRule(input, start, count);

	/// <summary>
	/// Creates an `all` rule.
	/// </summary>
	/// <param name="input">The input rule.</param>
	/// <param name="rule">The predicate to test by.</param>
	/// <returns>An `all` rule.</returns>
	public static Rule All(Rule input, Rule rule) => new AllRule(input, rule);
	/// <summary>
	/// Creates a `some` ("any") rule.
	/// </summary>
	/// <param name="input">The input rule.</param>
	/// <param name="rule">The predicate to test by.</param>
	/// <returns>A `some` rule.</returns>
	public static Rule Some(Rule input, Rule rule) => new SomeRule(input, rule);
	
	/// <summary>
	/// Creates a `none` rule.
	/// </summary>
	/// <param name="input">The input rule.</param>
	/// <param name="rule">The predicate to test by.</param>
	/// <returns>A `none` rule.</returns>
	public static Rule None(Rule input, Rule rule) => new NoneRule(input, rule);
	
	/// <summary>
	/// Creates a `missing` rule.
	/// </summary>
	/// <param name="components">The missing components.</param>
	/// <returns>A `missing` rule.</returns>
	public static Rule Missing(params Rule[] components) => new MissingRule(components);
	
	/// <summary>
	/// Creates a `missing-some` rule.
	/// </summary>
	/// <param name="requiredCount">The required count.</param>
	/// <param name="components">The missing components.</param>
	/// <returns>A `missing_some` rule.</returns>
	public static Rule MissingSome(Rule requiredCount, Rule components) => new MissingSomeRule(requiredCount, components);
	/// <summary>
	/// Creates a `none` rule.
	/// </summary>
	/// <param name="test">The predicate to test by.</param>
	/// <param name="input">The input rule.</param>
	/// <returns>A `none` rule.</returns>
	public static Rule In(Rule test, Rule input) => new InRule(test, input);

	/// <summary>
	/// Creates a `map` rule.
	/// </summary>
	/// <param name="input">The input rule.</param>
	/// <param name="rule">The predicate to test by.</param>
	/// <returns>A `map` rule.</returns>
	public static Rule Map(Rule input, Rule rule) => new MapRule(input, rule);
	/// <summary>
	/// Creates a `reduce` rule.
	/// </summary>
	/// <param name="input">The input rule.</param>
	/// <param name="rule">The predicate to test by.</param>
	/// <param name="initial">The initial rule.</param>
	/// <returns>A `reduce` rule.</returns>
	public static Rule Reduce(Rule input, Rule rule, Rule initial) => new ReduceRule(input, rule, initial);
	/// <summary>
	/// Creates a `filter` rule.
	/// </summary>
	/// <param name="input">The input rule.</param>
	/// <param name="rule">The predicate to test by.</param>
	/// <returns>A `filter` rule.</returns>
	public static Rule Filter(Rule input, Rule rule) => new FilterRule(input, rule);
	
	/// <summary>
	/// Creates a `merge` rule.
	/// </summary>
	/// <param name="items">The items to merge.</param>
	/// <returns>A `merge` rule.</returns>
	public static Rule Merge(params Rule[] items) => new MergeRule(items);

	/// <summary>
	/// Creates a rule that stands in for a literal JSON value.
	/// </summary>
	/// <param name="value">The JSON value.</param>
	/// <returns>A literal rule.</returns>
	public static Rule Literal(JsonNode? value) => new LiteralRule(value);

	/// <summary>
	/// Creates a `var` rule that accesses data.
	/// </summary>
	/// <param name="input">A rule that produces the dot-delimited path.</param>
	/// <returns>A variable rule.</returns>
	public static Rule Variable(Rule input) => new VariableRule(input);
	/// <summary>
	/// Creates a `var` rule that accesses data.
	/// </summary>
	/// <param name="input">A rule that produces the dot-delimited path.</param>
	/// <param name="defaultValue">A default value to use if the path is not found.</param>
	/// <returns>A variable rule.</returns>
	public static Rule Variable(Rule input, Rule defaultValue) => new VariableRule(input, defaultValue);
	/// <summary>
	/// Creates a `var` rule that accesses data.
	/// </summary>
	/// <param name="path">The dot-delimited path.</param>
	/// <returns>A variable rule.</returns>
	public static Rule Variable(string path) => new VariableRule(Literal(path));
	/// <summary>
	/// Creates a `var` rule that accesses data.
	/// </summary>
	/// <param name="path">The dot-delimited path.</param>
	/// <param name="defaultValue">A default value to use if the path is not found.</param>
	/// <returns>A variable rule.</returns>
	public static Rule Variable(string path, Rule defaultValue) => new VariableRule(Literal(path), defaultValue);

	/// <summary>
	/// Functions as a no-op.  Processes the rule, then logs and returns the output.
	/// </summary>
	/// <param name="log">The rule to log.</param>
	/// <returns>The result of the rule.</returns>
	public static Rule Log(Rule log) => new LogRule(log);

}