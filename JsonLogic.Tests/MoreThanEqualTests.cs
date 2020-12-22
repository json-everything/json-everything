using System.Text.Json;
using Json.Logic.Rules;
using Json.More;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class MoreThanEqualTests
	{
		[Test]
		public void MoreThanEqualTwoNumbersReturnsTrue()
		{
			var rule = new MoreThanEqualRule(2, 1);

			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void EqualTwoNumbersReturnsTrue()
		{
			var rule = new MoreThanEqualRule(1, 1);

			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void MoreThanEqualTwoNumbersReturnsFalse()
		{
			var rule = new MoreThanEqualRule(2, 3);

			JsonAssert.IsFalse(rule.Apply());
		}
		
		[Test]
		public void MoreThanEqualStringThrowsError()
		{
			var rule = new MoreThanEqualRule("foo", 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void MoreThanEqualBooleanThrowsError()
		{
			var rule = new MoreThanEqualRule(false, 2);

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void MoreThanEqualArrayThrowsError()
		{
			var rule = new MoreThanEqualRule(new JsonElement[]{}.AsJsonElement(), 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void MoreThanEqualObjectThrowsError()
		{
			var rule = new MoreThanEqualRule(JsonDocument.Parse("{}").RootElement, 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void MoreThanEqualNullThrowsError()
		{
			var rule = new MoreThanEqualRule(LiteralRule.Null, 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}