using System.Text.Json;
using Json.Logic.Rules;
using Json.More;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class CatTests
	{
		[Test]
		public void CatTwoStringsConcatsValues()
		{
			var rule = new CatRule("foo", "bar");
			
			JsonAssert.AreEquivalent("foobar", rule.Apply());
		}

		[Test]
		public void CatStringAndNullConcatsValues()
		{
			var rule = new CatRule("foo", LiteralRule.Null);
			
			JsonAssert.AreEquivalent("foo", rule.Apply());
		}

		[Test]
		public void CatStringAndNumberConcatsValues()
		{
			var rule = new CatRule("foo", 1);
			
			JsonAssert.AreEquivalent("foo1", rule.Apply());
		}

		[Test]
		public void CatStringAndBooleanConcatsValues()
		{
			var rule = new CatRule("foo", true);
			
			JsonAssert.AreEquivalent("footrue", rule.Apply());
		}

		[Test]
		public void CatStringAndArrayConcatsValues()
		{
			var array = new[]{1.AsJsonElement(),2.AsJsonElement(),3.AsJsonElement()}.AsJsonElement();
			var rule = new CatRule("foo", array);
			
			JsonAssert.AreEquivalent("foo1,2,3", rule.Apply());
		}

		[Test]
		public void CatStringAndNestedArrayConcatsValues()
		{
			var array = new[]{1.AsJsonElement(),2.AsJsonElement(),3.AsJsonElement()}.AsJsonElement();
			var nestedArray = new[]{1.AsJsonElement(),array,3.AsJsonElement()}.AsJsonElement();
			var rule = new CatRule("foo", nestedArray);

			JsonAssert.AreEquivalent("foo1,1,2,3,3", rule.Apply());
		}

		[Test]
		public void CatStringAndObjectConcatsValues()
		{
			var rule = new CatRule("foo", JsonDocument.Parse("{}").RootElement);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}