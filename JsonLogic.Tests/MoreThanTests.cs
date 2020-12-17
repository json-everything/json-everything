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
			var rule = new MoreThanComponent(new LiteralComponent(2), new LiteralComponent(1));

			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void EqualTwoNumbersReturnsFalse()
		{
			var rule = new MoreThanComponent(new LiteralComponent(1), new LiteralComponent(1));

			JsonAssert.IsFalse(rule.Apply());
		}
		
		[Test]
		public void MoreThanTwoNumbersReturnsFalse()
		{
			var rule = new MoreThanComponent(new LiteralComponent(1), new LiteralComponent(2));

			JsonAssert.IsFalse(rule.Apply());
		}
		
		[Test]
		public void MoreThanStringThrowsError()
		{
			var rule = new MoreThanComponent(new LiteralComponent("foo"), new LiteralComponent(2));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void MoreThanBooleanThrowsError()
		{
			var rule = new MoreThanComponent(new LiteralComponent(false), new LiteralComponent(2));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void MoreThanArrayThrowsError()
		{
			var rule = new MoreThanComponent(new LiteralComponent(new JsonElement[]{}.AsJsonElement()), new LiteralComponent(2));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void MoreThanObjectThrowsError()
		{
			var rule = new MoreThanComponent(new LiteralComponent(JsonDocument.Parse("{}").RootElement), new LiteralComponent(2));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void MoreThanNullThrowsError()
		{
			var rule = new MoreThanComponent(new LiteralComponent(null), new LiteralComponent(2));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}