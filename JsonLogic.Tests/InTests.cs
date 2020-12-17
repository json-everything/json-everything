using System.Text.Json;
using Json.Logic.Components;
using Json.More;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class InTests
	{
		[Test]
		public void InTwoStringsSecondContainsFirstReturnsTrue()
		{
			var rule = new InComponent(new LiteralComponent("foo"), new LiteralComponent("food"));
			
			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void InTwoStringsNoMatchReturnsFalse()
		{
			var rule = new InComponent(new LiteralComponent("foo"), new LiteralComponent("bar"));
			
			JsonAssert.IsFalse(rule.Apply());
		}
		
		[Test]
		public void InStringContainsNumberReturnsTrue()
		{
			var rule = new InComponent(new LiteralComponent(4), new LiteralComponent("foo4bar"));
			
			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void InStringContainsBooleanReturnsTrue()
		{
			var rule = new InComponent(new LiteralComponent(true), new LiteralComponent("footruebar"));
			
			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void InStringContainsNullReturnsFalse()
		{
			var rule = new InComponent(new LiteralComponent(true), new LiteralComponent("foo"));
			
			JsonAssert.IsFalse(rule.Apply());
		}
		
		[Test]
		public void InStringContainsObjectThrowsError()
		{
			var rule = new InComponent(new LiteralComponent(JsonDocument.Parse("{}").RootElement), new LiteralComponent("foo"));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void InArrayContainsFirstReturnsTrue()
		{
			var array = new[]{1.AsJsonElement(),2.AsJsonElement(),3.AsJsonElement()}.AsJsonElement();
			var rule = new InComponent(new LiteralComponent(2), new LiteralComponent(array));

			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void InArrayDoesNotContainFirstReturnsFalse()
		{
			var array = new[]{1.AsJsonElement(),2.AsJsonElement(),3.AsJsonElement()}.AsJsonElement();
			var rule = new InComponent(new LiteralComponent(5), new LiteralComponent(array));

			JsonAssert.IsFalse(rule.Apply());
		}
		
		[Test]
		public void InObjectThrowsError()
		{
			var rule = new InComponent(new LiteralComponent(1), new LiteralComponent(JsonDocument.Parse("{}").RootElement));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void InNullThrowsError()
		{
			var rule = new InComponent(new LiteralComponent(1), new LiteralComponent(null));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void InBooleanThrowsError()
		{
			var rule = new InComponent(new LiteralComponent(1), new LiteralComponent(false));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void InNumberThrowsError()
		{
			var rule = new InComponent(new LiteralComponent(1), new LiteralComponent(4));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}