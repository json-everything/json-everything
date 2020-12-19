using Json.Logic.Components;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class IfTests
	{
		[Test]
		public void IfStandardReturnsTrueResult()
		{
			var rule = new IfComponent(true, 1, 2);
			
			JsonAssert.AreEquivalent(1, rule.Apply());
		}
		
		[Test]
		public void IfStandardReturnsFalseResult()
		{
			var rule = new IfComponent(false, 1, 2);
			
			JsonAssert.AreEquivalent(2, rule.Apply());
		}
		
		[Test]
		public void IfStandardReturnsSecondTrueResult()
		{
			var rule = new IfComponent(false, 1, true, 2, 3);
			
			JsonAssert.AreEquivalent(2, rule.Apply());
		}
		
		[Test]
		public void IfStandardReturnsSecondFalseResult()
		{
			var rule = new IfComponent(false, 1, false, 2, 3);
			
			JsonAssert.AreEquivalent(3, rule.Apply());
		}
	}
}