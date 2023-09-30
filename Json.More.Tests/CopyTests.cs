using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.More.Tests;

public class CopyTests
{
	[Test]
	public void ComplexCopy()
	{
		JsonNode source = new JsonObject
		{
			["foo"] = new JsonArray { 1, false, "string", null, 5.4 },
			["bar"] = true,
			["baz"] = new JsonObject
			{
				["bif"] = "apple"
			}
		};

		var copy = source.Copy();

		Assert.AreNotSame(source, copy);
		Assert.IsTrue(source.IsEquivalentTo(copy));
	}
}