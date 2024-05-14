using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests;

public class ModTests
{
	[Test]
	public void ModNumbersReturnsSum()
	{
		var rule = new ModRule(4, 5);

		var actual = rule.Apply();
		JsonAssert.AreEquivalent(4, actual);
	}
}