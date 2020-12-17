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
			var rule = new MoreThanEqualComponent(new LiteralComponent(2), new LiteralComponent(1));

			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void EqualTwoNumbersReturnsTrue()
		{
			var rule = new MoreThanEqualComponent(new LiteralComponent(1), new LiteralComponent(1));

			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void MoreThanEqualTwoNumbersReturnsFalse()
		{
			var rule = new MoreThanEqualComponent(new LiteralComponent(2), new LiteralComponent(3));

			JsonAssert.IsFalse(rule.Apply());
		}
		
		[Test]
		public void MoreThanEqualStringThrowsError()
		{
			var rule = new MoreThanEqualComponent(new LiteralComponent("foo"), new LiteralComponent(2));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void MoreThanEqualBooleanThrowsError()
		{
			var rule = new MoreThanEqualComponent(new LiteralComponent(false), new LiteralComponent(2));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void MoreThanEqualArrayThrowsError()
		{
			var rule = new MoreThanEqualComponent(new LiteralComponent(new JsonElement[]{}.AsJsonElement()), new LiteralComponent(2));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void MoreThanEqualObjectThrowsError()
		{
			var rule = new MoreThanEqualComponent(new LiteralComponent(JsonDocument.Parse("{}").RootElement), new LiteralComponent(2));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void MoreThanEqualNullThrowsError()
		{
			var rule = new MoreThanEqualComponent(new LiteralComponent(null), new LiteralComponent(2));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}