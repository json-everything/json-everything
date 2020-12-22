using System.Text.Json;
using Json.Logic.Components;
using Json.More;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class MoreThanTests
	{
		[Test]
		public void MoreThanTwoNumbersReturnsTrue()
		{
			var rule = new MoreThanComponent(2, 1);

			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void EqualTwoNumbersReturnsFalse()
		{
			var rule = new MoreThanComponent(1, 1);

			JsonAssert.IsFalse(rule.Apply());
		}
		
		[Test]
		public void MoreThanTwoNumbersReturnsFalse()
		{
			var rule = new MoreThanComponent(1, 2);

			JsonAssert.IsFalse(rule.Apply());
		}
		
		[Test]
		public void MoreThanStringThrowsError()
		{
			var rule = new MoreThanComponent("foo", 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void MoreThanBooleanThrowsError()
		{
			var rule = new MoreThanComponent(false, 2);

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void MoreThanArrayThrowsError()
		{
			var rule = new MoreThanComponent(new JsonElement[]{}.AsJsonElement(), 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void MoreThanObjectThrowsError()
		{
			var rule = new MoreThanComponent(JsonDocument.Parse("{}").RootElement, 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void MoreThanNullThrowsError()
		{
			var rule = new MoreThanComponent(LiteralComponent.Null, 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}