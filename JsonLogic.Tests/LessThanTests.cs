using System.Text.Json;
using Json.Logic.Components;
using Json.More;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class LessThanTests
	{
		[Test]
		public void LessThanTwoNumbersReturnsTrue()
		{
			var rule = new LessThanComponent(1, 2);

			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void EqualTwoNumbersReturnsFalse()
		{
			var rule = new LessThanComponent(1, 1);

			JsonAssert.IsFalse(rule.Apply());
		}
		
		[Test]
		public void LessThanTwoNumbersReturnsFalse()
		{
			var rule = new LessThanComponent(3, 2);

			JsonAssert.IsFalse(rule.Apply());
		}
		
		[Test]
		public void LessThanStringThrowsError()
		{
			var rule = new LessThanComponent("foo", 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void LessThanBooleanThrowsError()
		{
			var rule = new LessThanComponent(false, 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void LessThanArrayThrowsError()
		{
			var rule = new LessThanComponent(new JsonElement[]{}.AsJsonElement(), 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void LessThanObjectThrowsError()
		{
			var rule = new LessThanComponent(JsonDocument.Parse("{}").RootElement, 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void LessThanNullThrowsError()
		{
			var rule = new LessThanComponent(LiteralComponent.Null, 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}