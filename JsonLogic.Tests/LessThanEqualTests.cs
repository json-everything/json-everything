using System.Text.Json;
using Json.Logic.Rules;
using Json.More;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class LessThanEqualTests
	{
		[Test]
		public void LessThanTwoNumbersReturnsTrue()
		{
			var rule = new LessThanEqualRule(1, 2);

			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void EqualTwoNumbersReturnsTrue()
		{
			var rule = new LessThanEqualRule(2, 2);

			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void LessThanEqualTwoNumbersReturnsFalse()
		{
			var rule = new LessThanEqualRule(3, 2);

			JsonAssert.IsFalse(rule.Apply());
		}
		
		[Test]
		public void LessThanEqualStringThrowsError()
		{
			var rule = new LessThanEqualRule("foo", 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void LessThanEqualBooleanThrowsError()
		{
			var rule = new LessThanEqualRule(false, 2);

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void LessThanEqualArrayThrowsError()
		{
			var rule = new LessThanEqualRule(new JsonElement[]{}.AsJsonElement(), 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void LessThanEqualObjectThrowsError()
		{
			var rule = new LessThanEqualRule(JsonDocument.Parse("{}").RootElement, 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void LessThanEqualNullThrowsError()
		{
			var rule = new LessThanEqualRule(LiteralRule.Null, 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}