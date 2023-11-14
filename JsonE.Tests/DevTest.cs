using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.JsonE.Tests;

public class DevTest
{
	[Test]
	public void Check()
	{
		var obj = new SortedDictionary<string, JsonNode?>()
		{
			["b"] = 3,
			["a"] = 4
		};

		var expected = @"{""a"":4,""b"":3}";

		var actual = JsonSerializer.Serialize(obj);

		Assert.AreEqual(expected, actual);
	}
}