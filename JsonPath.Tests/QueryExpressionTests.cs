using System.Text.Json;
using Json.More;
using Json.Path.QueryExpressions;
using NUnit.Framework;

namespace Json.Path.Tests
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
			Assert.IsTrue(9.AsJsonElement().IsEquivalentTo(exp.Evaluate(default)));
			Assert.AreEqual("4+5", exp.ToString());
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
			Assert.IsTrue("yesmaybe".AsJsonElement().IsEquivalentTo(exp.Evaluate(default)));
			Assert.AreEqual("\"yes\"+\"maybe\"", exp.ToString());
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
			Assert.AreEqual("\"yes\"+5", exp.ToString());
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
			Assert.AreEqual(default(JsonElement), exp.Evaluate(default));
			Assert.AreEqual("4/0", exp.ToString());
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
			Assert.IsTrue(1.6.AsJsonElement().IsEquivalentTo(exp.Evaluate(default)));
			Assert.AreEqual("8/5", exp.ToString());
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
			Assert.IsTrue(false.AsJsonElement().IsEquivalentTo(exp.Evaluate(default)));
			Assert.AreEqual("8<5", exp.ToString());
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
			Assert.IsTrue(true.AsJsonElement().IsEquivalentTo(exp.Evaluate(default)));
			Assert.AreEqual("4<5", exp.ToString());
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
			Assert.IsTrue(true.AsJsonElement().IsEquivalentTo(exp.Evaluate(default)));
			Assert.AreEqual("true&&true", exp.ToString());
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
			Assert.IsTrue(false.AsJsonElement().IsEquivalentTo(exp.Evaluate(default)));
			Assert.AreEqual("false&&true", exp.ToString());
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
			Assert.IsTrue(false.AsJsonElement().IsEquivalentTo(exp.Evaluate(default)));
			Assert.AreEqual("true&&false", exp.ToString());
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
			Assert.IsTrue(false.AsJsonElement().IsEquivalentTo(exp.Evaluate(default)));
			Assert.AreEqual("false&&false", exp.ToString());
		}

		[Test]
		public void LengthMinusOne()
		{
			var exp = new QueryExpressionNode(
				new QueryExpressionNode(JsonPath.Parse("@.length")),
				Operators.Subtraction,
				new QueryExpressionNode(1.AsJsonElement())
			);

			Assert.AreEqual(QueryExpressionType.InstanceDependent, exp.OutputType);
			Assert.IsTrue(2.AsJsonElement().IsEquivalentTo(exp.Evaluate(JsonDocument.Parse("[1,2,3]").RootElement)));
			Assert.AreEqual("@.length-1", exp.ToString());
		}
	}
}
