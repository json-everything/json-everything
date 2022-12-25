using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Path.Tests;

internal class DevTest
{
	[Test]
	public void Test()
	{
		var path = JsonPath.Parse("$[?length(@.foo)>3]");
		var doc = new JsonArray
		{
			new JsonObject { ["id"] = 1, ["foo"] = "alphabet" },
			new JsonObject { ["id"] = 2, ["foo"] = new JsonArray(1, 2, 3, 4) },
			new JsonObject { ["id"] = 3, ["foo"] = new JsonArray(1, 2) },
			new JsonObject { ["id"] = 4, ["foo"] = "by" },
			new JsonObject { ["id"] = 4, ["bar"] = "alphabet" },
			new JsonObject { ["id"] = 5, ["foo"] = new JsonObject { ["a"] = 1, ["b"] = 1, ["c"] = 1, ["d"] = 1 } },
		};

		var result = path.Evaluate(doc);

		var serialized = JsonSerializer.Serialize(result, new JsonSerializerOptions
		{
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		});

		Console.WriteLine(serialized);
	}
}