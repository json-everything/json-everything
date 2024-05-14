using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.More.Tests;

public class GetPathFromRootTests
{
	[Test]
	public void BasicPath()
	{
		var data = new JsonObject
		{
			["foo"] = new JsonArray(0, 1, 2, 3)
		};

		var target = data["foo"]![2]!;

		var expected = "$['foo'][2]";

		var actual = target.GetPathFromRoot();

		Assert.AreEqual(expected, actual);
	}

	[Test]
	public void BasicPathGetShorthand()
	{
		var data = new JsonObject
		{
			["foo"] = new JsonArray(0, 1, 2, 3)
		};

		var target = data["foo"]![2]!;

		var expected = "$.foo[2]";

		var actual = target.GetPathFromRoot(true);

		Assert.AreEqual(expected, actual);
	}

	[Test]
	public void PathWithSingleQuoteInKey()
	{
		var data = new JsonObject
		{
			["fo'o"] = new JsonArray(0, 1, 2, 3)
		};

		var target = data["fo'o"]![2]!;

		var expected = "$['fo\\'o'][2]";

		var actual = target.GetPathFromRoot();

		Assert.AreEqual(expected, actual);
	}

	[Test]
	public void PathWithDoubleQuoteInKey()
	{
		var data = new JsonObject
		{
			["fo\"o"] = new JsonArray(0, 1, 2, 3)
		};

		var target = data["fo\"o"]![2]!;

		var expected = "$['fo\"o'][2]";

		var actual = target.GetPathFromRoot();

		Assert.AreEqual(expected, actual);
	}
}