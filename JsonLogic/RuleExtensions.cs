using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.Logic.Rules;

namespace Json.Logic;

/// <summary>
/// Calls <see cref="Rule.Apply"/> with no data.
/// </summary>
public static class RuleExtensions
{
	/// <summary>
	/// Calls <see cref="Rule.Apply"/> with no data.
	/// </summary>
	/// <param name="rule">The rule.</param>
	/// <returns>The result.</returns>
	public static JsonNode? Apply(this Rule rule)
	{
		return rule.Apply(null);
	}

	/// <summary>
	/// Gets the collection of items within the rule.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>The collection of rules.</returns>
	public static IEnumerable<Rule> GetItems(this AddRule rule)
	{
		return rule.Items.AsReadOnly();
	}

	/// <summary>
	/// Gets the rule that returns the input.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetInput(this AllRule rule)
	{
		return rule.Input;
	}

	/// <summary>
	/// Gets the rule that the items in the input should follow.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetRule(this AllRule rule)
	{
		return rule.Rule;
	}

	/// <summary>
	/// Gets the collection of items within the rule.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>The collection of items.</returns>
	public static IEnumerable<Rule> GetItems(this AndRule rule)
	{
		return rule.Items.AsReadOnly();
	}

	/// <summary>
	/// Gets the rule that returns the value.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetValue(this BooleanCastRule rule)
	{
		return rule.Value;
	}

	/// <summary>
	/// Gets the collection of items within the rule.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>The collection of rules.</returns>
	public static IEnumerable<Rule> GetItems(this CatRule rule)
	{
		return rule.Items.AsReadOnly();
	}

	/// <summary>
	/// Gets the rule that returns the dividend.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetDividend(this DivideRule rule)
	{
		return rule.A;
	}

	/// <summary>
	/// Gets the rule that returns the divisor.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetDivisor(this DivideRule rule)
	{
		return rule.B;
	}

	/// <summary>
	/// Gets the rule that returns the input.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetInput(this FilterRule rule)
	{
		return rule.Input;
	}

	/// <summary>
	/// Gets the rule that the items in the input should follow.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetRule(this FilterRule rule)
	{
		return rule.Rule;
	}

	/// <summary>
	/// Gets the rule that returns the condition.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule? GetCondition(this IfRule rule)
	{
		return rule.Components.Count > 0 ? rule.Components[0] : null;
	}

	/// <summary>
	/// Gets the rule that returns the then requirement.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule? GetThen(this IfRule rule)
	{
		return rule.Components.Count > 1 ? rule.Components[1] : null;
	}

	/// <summary>
	/// Gets the rule that returns the else requirement.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule? GetElse(this IfRule rule)
	{
		return rule.Components.Count > 2 ? rule.Components[2] : null;
	}

	/// <summary>
	/// Gets the rule that returns the test.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetTest(this InRule rule)
	{
		return rule.Test;
	}

	/// <summary>
	/// Gets the rule that returns the value.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetValue(this InRule rule)
	{
		return rule.Value;
	}

	/// <summary>
	/// Gets the rule that returns the left operand.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetLeft(this LessThanEqualRule rule)
	{
		return rule.A;
	}

	/// <summary>
	/// Gets the rule that returns the middle operand, if there is one.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule? GetMiddle(this LessThanEqualRule rule)
	{
		return rule.C is null ? null : rule.B;
	}

	/// <summary>
	/// Gets the rule that returns the right operand.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetRight(this LessThanEqualRule rule)
	{
		return rule.C ?? rule.B;
	}

	/// <summary>
	/// Gets the rule that returns the left operand.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetLeft(this LessThanRule rule)
	{
		return rule.A;
	}

	/// <summary>
	/// Gets the rule that returns the middle operand, if there is one.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule? GetMiddle(this LessThanRule rule)
	{
		return rule.C is null ? null : rule.B;
	}

	/// <summary>
	/// Gets the rule that returns the right operand.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetRight(this LessThanRule rule)
	{
		return rule.C ?? rule.B;
	}

	/// <summary>
	/// Gets the rule that returns the value.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetValue(this LiteralRule rule)
	{
		return rule.Value;
	}

	/// <summary>
	/// Gets the rule that returns the log entry.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetValue(this LogRule rule)
	{
		return rule.Log;
	}

	/// <summary>
	/// Gets the rule that returns the left operand.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetLeft(this LooseEqualsRule rule)
	{
		return rule.A;
	}

	/// <summary>
	/// Gets the rule that returns the right operand.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetRight(this LooseEqualsRule rule)
	{
		return rule.B;
	}

	/// <summary>
	/// Gets the rule that returns the left operand.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetLeft(this LooseNotEqualsRule rule)
	{
		return rule.A;
	}

	/// <summary>
	/// Gets the rule that returns the right operand.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetRight(this LooseNotEqualsRule rule)
	{
		return rule.B;
	}

	/// <summary>
	/// Gets the rule that returns the input.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetInput(this MapRule rule)
	{
		return rule.Input;
	}

	/// <summary>
	/// Gets the rule to process the items in the input.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetRule(this MapRule rule)
	{
		return rule.Rule;
	}

	/// <summary>
	/// Gets the collection of items within the rule.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>The collection of rules.</returns>
	public static IEnumerable<Rule> GetItems(this MaxRule rule)
	{
		return rule.Items.AsReadOnly();
	}

	/// <summary>
	/// Gets the collection of items within the rule.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>The collection of rules.</returns>
	public static IEnumerable<Rule> GetItems(this MergeRule rule)
	{
		return rule.Items.AsReadOnly();
	}

	/// <summary>
	/// Gets the collection of items within the rule.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>The collection of rules.</returns>
	public static IEnumerable<Rule> GetItems(this MinRule rule)
	{
		return rule.Items.AsReadOnly();
	}

	/// <summary>
	/// Gets the collection of items within the rule.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>The collection of rules.</returns>
	public static IEnumerable<Rule> GetItems(this MissingRule rule)
	{
		return rule.Components;
	}

	/// <summary>
	/// Gets the rule that returns the required count.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetRequiredCount(this MissingSomeRule rule)
	{
		return rule.RequiredCount;
	}

	/// <summary>
	/// Gets the rule that returns the items collection.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetItems(this MissingSomeRule rule)
	{
		return rule.Components;
	}

	/// <summary>
	/// Gets the rule that returns the dividend.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetDividend(this ModRule rule)
	{
		return rule.A;
	}

	/// <summary>
	/// Gets the rule that returns the divisor.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetDivisor(this ModRule rule)
	{
		return rule.B;
	}

	/// <summary>
	/// Gets the rule that returns the left operand.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetLeft(this MoreThanEqualRule rule)
	{
		return rule.A;
	}

	/// <summary>
	/// Gets the rule that returns the right operand.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetRight(this MoreThanEqualRule rule)
	{
		return rule.B;
	}

	/// <summary>
	/// Gets the rule that returns the left operand.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetLeft(this MoreThanRule rule)
	{
		return rule.A;
	}

	/// <summary>
	/// Gets the rule that returns the right operand.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetRight(this MoreThanRule rule)
	{
		return rule.B;
	}

	/// <summary>
	/// Gets the collection of items within the rule.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>The collection of rules.</returns>
	public static IEnumerable<Rule> GetItems(this MultiplyRule rule)
	{
		return rule.Items;
	}

	/// <summary>
	/// Gets the rule that returns the input.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetInput(this NoneRule rule)
	{
		return rule.Input;
	}

	/// <summary>
	/// Gets the rule that the items in the input should not follow.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetRule(this NoneRule rule)
	{
		return rule.Rule;
	}

	/// <summary>
	/// Gets the rule that returns the value.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetValue(this NotRule rule)
	{
		return rule.Value;
	}

	/// <summary>
	/// Gets the collection of items within the rule.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>The collection of rules.</returns>
	public static IEnumerable<Rule> GetItems(this OrRule rule)
	{
		return rule.Items;
	}

	/// <summary>
	/// Gets the rule that returns the input.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetInput(this ReduceRule rule)
	{
		return rule.Input;
	}

	/// <summary>
	/// Gets the rule to process the items in the input.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetRule(this ReduceRule rule)
	{
		return rule.Rule;
	}

	/// <summary>
	/// Gets the rule that returns the initial value.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetInitial(this ReduceRule rule)
	{
		return rule.Initial;
	}

	/// <summary>
	/// Gets the collection of items within the rule.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>The collection of rules.</returns>
	public static IEnumerable<Rule> GetItems(this RuleCollection rule)
	{
		return rule.Rules;
	}

	/// <summary>
	/// Gets the rule that returns the input.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetInput(this SomeRule rule)
	{
		return rule.Input;
	}

	/// <summary>
	/// Gets the rule that the items in the input should follow.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetRule(this SomeRule rule)
	{
		return rule.Rule;
	}

	/// <summary>
	/// Gets the rule that returns the left operand.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetLeft(this StrictEqualsRule rule)
	{
		return rule.A;
	}

	/// <summary>
	/// Gets the rule that returns the right operand.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetRight(this StrictEqualsRule rule)
	{
		return rule.B;
	}

	/// <summary>
	/// Gets the rule that returns the left operand.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetLeft(this StrictNotEqualsRule rule)
	{
		return rule.A;
	}

	/// <summary>
	/// Gets the rule that returns the right operand.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetRight(this StrictNotEqualsRule rule)
	{
		return rule.B;
	}

	/// <summary>
	/// Gets the rule that returns the input.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetInput(this SubstrRule rule)
	{
		return rule.Input;
	}

	/// <summary>
	/// Gets the rule that returns the start index.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule GetStart(this SubstrRule rule)
	{
		return rule.Start;
	}

	/// <summary>
	/// Gets the rule that returns the character count.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule? GetCount(this SubstrRule rule)
	{
		return rule.Count;
	}

	/// <summary>
	/// Gets the collection of items within the rule.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>The collection of rules.</returns>
	public static IEnumerable<Rule> GetItems(this SubtractRule rule)
	{
		return rule.Items;
	}

	/// <summary>
	/// Gets the rule that returns the path.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule? GetPath(this VariableRule rule)
	{
		return rule.Path;
	}

	/// <summary>
	/// Gets the rule that returns the default value.
	/// </summary>
	/// <param name="rule">The rule</param>
	/// <returns>A rule.</returns>
	public static Rule? GetDefault(this VariableRule rule)
	{
		return rule.DefaultValue;
	}
}