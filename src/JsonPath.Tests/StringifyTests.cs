using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using NUnit.Framework;

namespace Json.Path.Tests;

public class StringifyTests
{
	private static PathParsingOptions _options = new()
	{
		AllowInOperator = true,
		AllowJsonConstructs = true,
		AllowMathOperations = true,
		AllowRelativePathStart = true,
		TolerateExtraWhitespace = true
	};

	public static void AssertStringify(string pathText, JsonNode? data, PathParsingOptions? options = null)
	{
		Console.WriteLine("Original Path:   {0}", pathText);
		var path = JsonPath.Parse(pathText, options);
		var originalResult = path.Evaluate(data);

		var backToString = path.ToString();
		Console.WriteLine("New Path:        {0}", backToString);
		if (!JsonPath.TryParse(backToString, out var newPath, options))
			Assert.Inconclusive("Stringified semantics do not match original string");

		var newResult = newPath.Evaluate(data);
		Console.WriteLine("Original Result: {0}", JsonSerializer.Serialize(originalResult.Matches.Select(x => x.Value)));
		Console.WriteLine("New Result:      {0}", JsonSerializer.Serialize(newResult.Matches.Select(x => x.Value)));

		if (originalResult.Matches.Count != newResult.Matches.Count)
			Assert.Inconclusive("Stringified semantics do not match original string");

		foreach (var (o, n) in originalResult.Matches.Zip(newResult.Matches))
		{
			if (!o.Value.IsEquivalentTo(n.Value))
				Assert.Inconclusive("Stringified semantics do not match original string");
		}
	}

	[TestCase("$[?@ in [42, 43, 44]]", "[1, 2, 43, 54, 69]")]
	public void InExpression(string pathText, string dataText)
	{
		var data = JsonNode.Parse(dataText);

		AssertStringify(pathText, data, _options);
	}
}