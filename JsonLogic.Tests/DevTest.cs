using NUnit.Framework;

using static Json.Logic.JsonLogic;

namespace Json.Logic.Tests;

public class DevTest
{
	[Test]
	public void Check()
	{
		And(
			StrictEquals(5, 10),
			Literal(true)
		);
	}
}