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
			var rule = new InComponent("foo", "food");
			
			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void InTwoStringsNoMatchReturnsFalse()
		{
			var rule = new InComponent("foo", "bar");
			
			JsonAssert.IsFalse(rule.Apply());
		}
		
		[Test]
		public void InStringContainsNumberReturnsTrue()
		{
			var rule = new InComponent(4, "foo4bar");
			
			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void InStringContainsBooleanReturnsTrue()
		{
			var rule = new InComponent(true, "footruebar");
			
			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void InStringContainsNullReturnsFalse()
		{
			var rule = new InComponent(true, "foo");
			
			JsonAssert.IsFalse(rule.Apply());
		}
		
		[Test]
		public void InStringContainsObjectThrowsError()
		{
			var rule = new InComponent(JsonDocument.Parse("{}").RootElement, "foo");

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void InArrayContainsFirstReturnsTrue()
		{
			var array = new[]{1.AsJsonElement(),2.AsJsonElement(),3.AsJsonElement()}.AsJsonElement();
			var rule = new InComponent(2, array);

			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void InArrayDoesNotContainFirstReturnsFalse()
		{
			var array = new[]{1.AsJsonElement(),2.AsJsonElement(),3.AsJsonElement()}.AsJsonElement();
			var rule = new InComponent(5, array);

			JsonAssert.IsFalse(rule.Apply());
		}
		
		[Test]
		public void InObjectThrowsError()
		{
			var rule = new InComponent(1, JsonDocument.Parse("{}").RootElement);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void InNullThrowsError()
		{
			var rule = new InComponent(1, LiteralComponent.Null);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void InBooleanThrowsError()
		{
			var rule = new InComponent(1, false);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void InNumberThrowsError()
		{
			var rule = new InComponent(1, 4);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}