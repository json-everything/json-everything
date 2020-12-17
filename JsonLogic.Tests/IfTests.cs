using Json.Logic.Components;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class IfTests
	{
		[Test]
		public void IfStandardReturnsTrueResult()
		{
			var rule = new IfComponent(new LiteralComponent(true), new LiteralComponent(1), new LiteralComponent(2));
			
			JsonAssert.AreEquivalent(1, rule.Apply());
		}
		
		[Test]
		public void IfStandardReturnsFalseResult()
		{
			var rule = new IfComponent(new LiteralComponent(false), new LiteralComponent(1), new LiteralComponent(2));
			
			JsonAssert.AreEquivalent(2, rule.Apply());
		}
		
		[Test]
		public void IfStandardReturnsSecondTrueResult()
		{
			var rule = new IfComponent(new LiteralComponent(false), new LiteralComponent(1),
				new LiteralComponent(true), new LiteralComponent(2),
				new LiteralComponent(3));
			
			JsonAssert.AreEquivalent(2, rule.Apply());
		}
		
		[Test]
		public void IfStandardReturnsSecondFalseResult()
		{
			var rule = new IfComponent(new LiteralComponent(false), new LiteralComponent(1),
				new LiteralComponent(false), new LiteralComponent(2),
				new LiteralComponent(3));
			
			JsonAssert.AreEquivalent(3, rule.Apply());
		}
	}
}