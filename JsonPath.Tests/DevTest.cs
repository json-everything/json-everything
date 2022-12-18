using NUnit.Framework;

namespace Json.Path.Tests;

internal class DevTest
{
	[Test]
	public void Test()
	{
		var pathText = "$['foo']";

		var path = JsonPath.Parse(pathText);
	}
}