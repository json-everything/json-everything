using System;
using System.Linq;
using System.Linq.Expressions;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests;

public class StrictEqualsTests
{
	[Test]
	public void NotEqualReturnsFalse()
	{
		var rule = new StrictEqualsRule(1, 2);

		JsonAssert.IsFalse(rule.Apply());
	}

	[Test]
	public void EqualsReturnsTrue()
	{
		var rule = new StrictEqualsRule(1, 1);

		JsonAssert.IsTrue(rule.Apply());
	}

	[Test]
	public void LooseEqualsReturnsFalse()
	{
		var rule = new StrictEqualsRule(1, "1");

		JsonAssert.IsFalse(rule.Apply());
	}


	[Test]
	public void BuildExpression()
	{
		var parameter = Expression.Parameter(typeof(TestData), "x");
		var literalRule = new LiteralRule("Hello");
		var literalExpression = literalRule.BuildExpressionPredicate<TestData>(parameter);

		var variableRule = new VariableRule("Name");
		var variableExpression = variableRule.BuildExpressionPredicate<TestData>(parameter);

		var rule = new StrictEqualsRule(variableRule, literalRule);
		var rule2 = new StrictEqualsRule(variableRule, new LiteralRule("Hai"));
		var rule3 = new StrictEqualsRule(variableRule, new LiteralRule("Hola"));

		var orRule = new OrRule(rule, rule2, rule3);

		var expression = orRule.BuildExpressionPredicate<TestData>(parameter);
		Console.WriteLine(expression);

		var data = GetData();

		var lambda = Expression.Lambda<Func<TestData, bool>>(expression, parameter);
		var query = data.Where(lambda);
		var results = query.ToList();

		Assert.That(results, Has.Count.EqualTo(3));
		Assert.That(expression.ToString(), Is.EqualTo("(((x.Name == \"Hello\") OrElse (x.Name == \"Hai\")) OrElse (x.Name == \"Hola\"))"));
	}

	private static IQueryable<TestData> GetData()
	{
		return new TestData[]
		{
			new TestData() { Name = "Hello" },
			new TestData() { Name = "Hai" },
			new TestData() { Name = "Bonjour" },
			new TestData() { Name = "Hola" },
			new TestData() { Name = "Buongiorno" },
		}.AsQueryable();
	}

	public class TestData
	{
		public string Name { get; set; }
	}
}