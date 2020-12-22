using System.Text.Json;
using Json.Logic.Components;
using Json.More;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class MoreThanEqualTests
	{
		[Test]
		public void MoreThanEqualTwoNumbersReturnsTrue()
		{
			var rule = new MoreThanEqualComponent(2, 1);

			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void EqualTwoNumbersReturnsTrue()
		{
			var rule = new MoreThanEqualComponent(1, 1);

			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void MoreThanEqualTwoNumbersReturnsFalse()
		{
			var rule = new MoreThanEqualComponent(2, 3);

			JsonAssert.IsFalse(rule.Apply());
		}
		
		[Test]
		public void MoreThanEqualStringThrowsError()
		{
			var rule = new MoreThanEqualComponent("foo", 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void MoreThanEqualBooleanThrowsError()
		{
			var rule = new MoreThanEqualComponent(false, 2);

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void MoreThanEqualArrayThrowsError()
		{
			var rule = new MoreThanEqualComponent(new JsonElement[]{}.AsJsonElement(), 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void MoreThanEqualObjectThrowsError()
		{
			var rule = new MoreThanEqualComponent(JsonDocument.Parse("{}").RootElement, 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void MoreThanEqualNullThrowsError()
		{
			var rule = new MoreThanEqualComponent(LiteralComponent.Null, 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}