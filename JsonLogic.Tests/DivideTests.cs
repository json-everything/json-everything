using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests;

public class DivideTests
{
	[Test]
	public void DivideNumbersReturnsSum()
	{
		var rule = new DivideRule(4, 5);

		var actual = rule.Apply();
		JsonAssert.AreEquivalent(.8, actual);
	}
}