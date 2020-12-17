using Json.Logic.Components;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class SubstrTests
	{
		[Test]
		public void SubstrStartNoCount()
		{
			var rule = new SubstrComponent(new LiteralComponent("foobar"), new LiteralComponent(3));
			
			JsonAssert.AreEquivalent("bar", rule.Apply());
		}
		
		[Test]
		public void SubstrStartBeyondLengthNoCount()
		{
			var rule = new SubstrComponent(new LiteralComponent("foobar"), new LiteralComponent(10));

			JsonAssert.AreEquivalent(string.Empty, rule.Apply());
		}

		[Test]
		public void SubstrNegativeStartNoCount()
		{
			var rule = new SubstrComponent(new LiteralComponent("foobar"), new LiteralComponent(-2));
			
			JsonAssert.AreEquivalent("ar", rule.Apply());
		}
		
		[Test]
		public void SubstrNegativeStartBeyondLengthNoCount()
		{
			var rule = new SubstrComponent(new LiteralComponent("foobar"), new LiteralComponent(-10));

			JsonAssert.AreEquivalent("foobar", rule.Apply());
		}

		[Test]
		public void SubstrStartCount()
		{
			var rule = new SubstrComponent(new LiteralComponent("foobar"), new LiteralComponent(3), new LiteralComponent(2));
			
			JsonAssert.AreEquivalent("ba", rule.Apply());
		}
		
		[Test]
		public void SubstrStartCountBeyondLength()
		{
			var rule = new SubstrComponent(new LiteralComponent("foobar"), new LiteralComponent(3), new LiteralComponent(5));

			JsonAssert.AreEquivalent("bar", rule.Apply());
		}

		[Test]
		public void SubstrStartNegativeCount()
		{
			var rule = new SubstrComponent(new LiteralComponent("foobar"), new LiteralComponent(2), new LiteralComponent(-1));
			
			JsonAssert.AreEquivalent("oba", rule.Apply());
		}
		
		[Test]
		public void SubstrStartNegativeCountBeyondLength()
		{
			var rule = new SubstrComponent(new LiteralComponent("foobar"), new LiteralComponent(2), new LiteralComponent(-10));

			JsonAssert.AreEquivalent(string.Empty, rule.Apply());
		}
	}
}