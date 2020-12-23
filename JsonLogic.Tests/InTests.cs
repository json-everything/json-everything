using System.Text.Json;
using Json.Logic.Rules;
using Json.More;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class InTests
	{
		[Test]
		public void InTwoStringsSecondContainsFirstReturnsTrue()
		{
			var rule = new InRule("foo", "food");
			
			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void InTwoStringsNoMatchReturnsFalse()
		{
			var rule = new InRule("foo", "bar");
			
			JsonAssert.IsFalse(rule.Apply());
		}
		
		[Test]
		public void InStringContainsNumberReturnsTrue()
		{
			var rule = new InRule(4, "foo4bar");
			
			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void InStringContainsBooleanReturnsTrue()
		{
			var rule = new InRule(true, "footruebar");
			
			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void InStringContainsNullReturnsFalse()
		{
			var rule = new InRule(true, "foo");
			
			JsonAssert.IsFalse(rule.Apply());
		}
		
		[Test]
		public void InStringContainsObjectThrowsError()
		{
			var rule = new InRule(JsonDocument.Parse("{}").RootElement, "foo");

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void InArrayContainsFirstReturnsTrue()
		{
			var array = new[]{1.AsJsonElement(),2.AsJsonElement(),3.AsJsonElement()}.AsJsonElement();
			var rule = new InRule(2, array);

			JsonAssert.IsTrue(rule.Apply());
		}
		
		[Test]
		public void InArrayDoesNotContainFirstReturnsFalse()
		{
			var array = new[]{1.AsJsonElement(),2.AsJsonElement(),3.AsJsonElement()}.AsJsonElement();
			var rule = new InRule(5, array);

			JsonAssert.IsFalse(rule.Apply());
		}
		
		[Test]
		public void InObjectThrowsError()
		{
			var rule = new InRule(1, JsonDocument.Parse("{}").RootElement);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void InNullThrowsError()
		{
			var rule = new InRule(1, LiteralRule.Null);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void InBooleanThrowsError()
		{
			var rule = new InRule(1, false);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
		
		[Test]
		public void InNumberThrowsError()
		{
			var rule = new InRule(1, 4);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}