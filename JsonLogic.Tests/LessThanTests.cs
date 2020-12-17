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
			var rule = new LessThanComponent(new LiteralComponent(1), new LiteralComponent(2));

			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void EqualTwoNumbersReturnsFalse()
		{
			var rule = new LessThanComponent(new LiteralComponent(1), new LiteralComponent(1));

			JsonAssert.IsFalse(rule.Apply());
		}
		
		[Test]
		public void LessThanTwoNumbersReturnsFalse()
		{
			var rule = new LessThanComponent(new LiteralComponent(3), new LiteralComponent(2));

			JsonAssert.IsFalse(rule.Apply());
		}
		
		[Test]
		public void LessThanStringThrowsError()
		{
			var rule = new LessThanComponent(new LiteralComponent("foo"), new LiteralComponent(2));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void LessThanBooleanThrowsError()
		{
			var rule = new LessThanComponent(new LiteralComponent(false), new LiteralComponent(2));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void LessThanArrayThrowsError()
		{
			var rule = new LessThanComponent(new LiteralComponent(new JsonElement[]{}.AsJsonElement()), new LiteralComponent(2));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void LessThanObjectThrowsError()
		{
			var rule = new LessThanComponent(new LiteralComponent(JsonDocument.Parse("{}").RootElement), new LiteralComponent(2));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void LessThanNullThrowsError()
		{
			var rule = new LessThanComponent(new LiteralComponent(null), new LiteralComponent(2));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}