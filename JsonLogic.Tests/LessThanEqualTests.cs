using System.Text.Json;
using Json.Logic.Components;
using Json.More;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class LessThanEqualTests
	{
		[Test]
		public void LessThanTwoNumbersReturnsTrue()
		{
			var rule = new LessThanEqualComponent(1, 2);

			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void EqualTwoNumbersReturnsTrue()
		{
			var rule = new LessThanEqualComponent(2, 2);

			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void LessThanEqualTwoNumbersReturnsFalse()
		{
			var rule = new LessThanEqualComponent(3, 2);

			JsonAssert.IsFalse(rule.Apply());
		}
		
		[Test]
		public void LessThanEqualStringThrowsError()
		{
			var rule = new LessThanEqualComponent("foo", 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void LessThanEqualBooleanThrowsError()
		{
			var rule = new LessThanEqualComponent(false, 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void LessThanEqualArrayThrowsError()
		{
			var rule = new LessThanEqualComponent(new JsonElement[]{}.AsJsonElement(), 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void LessThanEqualObjectThrowsError()
		{
			var rule = new LessThanEqualComponent(JsonDocument.Parse("{}").RootElement, 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void LessThanEqualNullThrowsError()
		{
			var rule = new LessThanEqualComponent(null, 2);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}