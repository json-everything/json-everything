using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class SubstrTests
	{
		[Test]
		public void SubstrStartNoCount()
		{
			var rule = new SubstrRule("foobar", 3);
			
			JsonAssert.AreEquivalent("bar", rule.Apply());
		}
		
		[Test]
		public void SubstrStartBeyondLengthNoCount()
		{
			var rule = new SubstrRule("foobar", 10);

			JsonAssert.AreEquivalent(string.Empty, rule.Apply());
		}

		[Test]
		public void SubstrNegativeStartNoCount()
		{
			var rule = new SubstrRule("foobar", -2);
			
			JsonAssert.AreEquivalent("ar", rule.Apply());
		}
		
		[Test]
		public void SubstrNegativeStartBeyondLengthNoCount()
		{
			var rule = new SubstrRule("foobar", -10);

			JsonAssert.AreEquivalent("foobar", rule.Apply());
		}

		[Test]
		public void SubstrStartCount()
		{
			var rule = new SubstrRule("foobar", 3, 2);
			
			JsonAssert.AreEquivalent("ba", rule.Apply());
		}
		
		[Test]
		public void SubstrStartCountBeyondLength()
		{
			var rule = new SubstrRule("foobar", 3, 5);

			JsonAssert.AreEquivalent("bar", rule.Apply());
		}

		[Test]
		public void SubstrStartNegativeCount()
		{
			var rule = new SubstrRule("foobar", 2, -1);
			
			JsonAssert.AreEquivalent("oba", rule.Apply());
		}
		
		[Test]
		public void SubstrStartNegativeCountBeyondLength()
		{
			var rule = new SubstrRule("foobar", 2, -10);

			JsonAssert.AreEquivalent(string.Empty, rule.Apply());
		}
	}
}