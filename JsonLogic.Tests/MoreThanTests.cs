using System.Text.Json;
using Json.Logic.Rules;
using Json.More;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class MoreThanTests
	{
		[Test]
		public void MoreThanTwoNumbersReturnsTrue()
		{
			var rule = new MoreThanRule(2, 1);

			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void EqualTwoNumbersReturnsFalse()
		{
			var rule = new MoreThanRule(1, 1);

			JsonAssert.IsFalse(rule.Apply());
		}
		
		[Test]
		public void MoreThanTwoNumbersReturnsFalse()
		{
			var rule = new MoreThanRule(1, 2);

			JsonAssert.IsFalse(rule.Apply());
		}
		
		[Test]
		public void MoreThanStringThrowsError()
		{
			var rule = new MoreThanRule("foo", 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void MoreThanBooleanThrowsError()
		{
			var rule = new MoreThanRule(false, 2);

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void MoreThanArrayThrowsError()
		{
			var rule = new MoreThanRule(new JsonElement[]{}.AsJsonElement(), 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void MoreThanObjectThrowsError()
		{
			var rule = new MoreThanRule(JsonDocument.Parse("{}").RootElement, 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void MoreThanNullThrowsError()
		{
			var rule = new MoreThanRule(LiteralRule.Null, 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}