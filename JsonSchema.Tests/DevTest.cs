using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class DevTest
{
	[Test]
	public void Test()
	{
		var text = "{\"type\":\"array\",\"items\":\"https://example.com/schema\"}";

		var schema = JsonSerializer.Deserialize<JsonSchema>(text)!;

		var backToText = JsonSerializer.Serialize(schema);

		Assert.AreEqual(text, backToText);
	}
}