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
			var rule = new LessThanEqualComponent(new LiteralComponent(1), new LiteralComponent(2));

			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void EqualTwoNumbersReturnsTrue()
		{
			var rule = new LessThanEqualComponent(new LiteralComponent(2), new LiteralComponent(2));

			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void LessThanEqualTwoNumbersReturnsFalse()
		{
			var rule = new LessThanEqualComponent(new LiteralComponent(3), new LiteralComponent(2));

			JsonAssert.IsFalse(rule.Apply());
		}
		
		[Test]
		public void LessThanEqualStringThrowsError()
		{
			var rule = new LessThanEqualComponent(new LiteralComponent("foo"), new LiteralComponent(2));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void LessThanEqualBooleanThrowsError()
		{
			var rule = new LessThanEqualComponent(new LiteralComponent(false), new LiteralComponent(2));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void LessThanEqualArrayThrowsError()
		{
			var rule = new LessThanEqualComponent(new LiteralComponent(new JsonElement[]{}.AsJsonElement()), new LiteralComponent(2));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void LessThanEqualObjectThrowsError()
		{
			var rule = new LessThanEqualComponent(new LiteralComponent(JsonDocument.Parse("{}").RootElement), new LiteralComponent(2));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void LessThanEqualNullThrowsError()
		{
			var rule = new LessThanEqualComponent(new LiteralComponent(null), new LiteralComponent(2));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}