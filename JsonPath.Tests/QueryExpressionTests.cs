using System.Text.Json;
using Json.More;
using Json.Path.QueryExpressions;
using NUnit.Framework;

namespace JsonPath.Tests
{
	public class QueryExpressionTests
	{
		[Test]
		public void NumberAddition()
		{
			var exp = new QueryExpressionNode(
				new QueryExpressionNode(4.AsJsonElement()),
				Operators.Addition,
				new QueryExpressionNode(5.AsJsonElement())
			);

			Assert.AreEqual(QueryExpressionType.Number, exp.OutputType);
			Assert.IsTrue(9.AsJsonElement().IsEquivalentTo(exp.Value));
		}

		[Test]
		public void StringAddition()
		{
			var exp = new QueryExpressionNode(
				new QueryExpressionNode("yes".AsJsonElement()),
				Operators.Addition,
				new QueryExpressionNode("maybe".AsJsonElement())
			);

			Assert.AreEqual(QueryExpressionType.String, exp.OutputType);
			Assert.IsTrue("yesmaybe".AsJsonElement().IsEquivalentTo(exp.Value));
		}

		[Test]
		public void MixedAddition()
		{
			var exp = new QueryExpressionNode(
				new QueryExpressionNode("yes".AsJsonElement()),
				Operators.Addition,
				new QueryExpressionNode(5.AsJsonElement())
			);

			Assert.AreEqual(QueryExpressionType.Invalid, exp.OutputType);
		}

		[Test]
		public void DivisionByZero()
		{
			var exp = new QueryExpressionNode(
				new QueryExpressionNode(4.AsJsonElement()),
				Operators.Division,
				new QueryExpressionNode(0.AsJsonElement())
			);

			Assert.AreEqual(QueryExpressionType.Number, exp.OutputType);
			Assert.AreEqual(default(JsonElement), exp.Value);
		}

		[Test]
		public void Division()
		{
			var exp = new QueryExpressionNode(
				new QueryExpressionNode(8.AsJsonElement()),
				Operators.Division,
				new QueryExpressionNode(5.AsJsonElement())
			);

			Assert.AreEqual(QueryExpressionType.Number, exp.OutputType);
			Assert.IsTrue(1.6.AsJsonElement().IsEquivalentTo(exp.Value));
		}

		[Test]
		public void LessThan_False()
		{
			var exp = new QueryExpressionNode(
				new QueryExpressionNode(8.AsJsonElement()),
				Operators.LessThan,
				new QueryExpressionNode(5.AsJsonElement())
			);

			Assert.AreEqual(QueryExpressionType.Boolean, exp.OutputType);
			Assert.IsTrue(false.AsJsonElement().IsEquivalentTo(exp.Value));
		}

		[Test]
		public void LessThan_True()
		{
			var exp = new QueryExpressionNode(
				new QueryExpressionNode(4.AsJsonElement()),
				Operators.LessThan,
				new QueryExpressionNode(5.AsJsonElement())
			);

			Assert.AreEqual(QueryExpressionType.Boolean, exp.OutputType);
			Assert.IsTrue(true.AsJsonElement().IsEquivalentTo(exp.Value));
		}

		[Test]
		public void And_True_True()
		{
			var exp = new QueryExpressionNode(
				new QueryExpressionNode(true.AsJsonElement()),
				Operators.And,
				new QueryExpressionNode(true.AsJsonElement())
			);

			Assert.AreEqual(QueryExpressionType.Boolean, exp.OutputType);
			Assert.IsTrue(true.AsJsonElement().IsEquivalentTo(exp.Value));
		}

		[Test]
		public void And_False_True()
		{
			var exp = new QueryExpressionNode(
				new QueryExpressionNode(false.AsJsonElement()),
				Operators.And,
				new QueryExpressionNode(true.AsJsonElement())
			);

			Assert.AreEqual(QueryExpressionType.Boolean, exp.OutputType);
			Assert.IsTrue(false.AsJsonElement().IsEquivalentTo(exp.Value));
		}

		[Test]
		public void And_True_False()
		{
			var exp = new QueryExpressionNode(
				new QueryExpressionNode(true.AsJsonElement()),
				Operators.And,
				new QueryExpressionNode(false.AsJsonElement())
			);

			Assert.AreEqual(QueryExpressionType.Boolean, exp.OutputType);
			Assert.IsTrue(false.AsJsonElement().IsEquivalentTo(exp.Value));
		}

		[Test]
		public void And_False_False()
		{
			var exp = new QueryExpressionNode(
				new QueryExpressionNode(false.AsJsonElement()),
				Operators.And,
				new QueryExpressionNode(false.AsJsonElement())
			);

			Assert.AreEqual(QueryExpressionType.Boolean, exp.OutputType);
			Assert.IsTrue(false.AsJsonElement().IsEquivalentTo(exp.Value));
		}
	}
}
