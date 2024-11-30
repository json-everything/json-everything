using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using NUnit.Framework;
using TestHelpers;

namespace Json.Path.Tests;

public class StringifyTests
{
	private static readonly PathParsingOptions _options = new()
	{
		AllowInOperator = true,
		AllowJsonConstructs = true,
		AllowMathOperations = true,
		AllowRelativePathStart = true,
		TolerateExtraWhitespace = true
	};

	public static void AssertStringify(string pathText, JsonNode? data, PathParsingOptions? options = null)
	{
		TestConsole.WriteLine();
		TestConsole.WriteLine("Original Path:   {0}", pathText);
		if (!JsonPath.TryParse(pathText, out var path, options))
			Assert.Inconclusive("Cannot parse original string");
		var originalResult = path.Evaluate(data);

		var backToString = path.ToString();
		TestConsole.WriteLine("New Path:        {0}", backToString);
		if (!JsonPath.TryParse(backToString, out var newPath, options))
			Assert.Inconclusive("Stringified semantics do not match original string");

		var newResult = newPath.Evaluate(data);
		TestConsole.WriteLine("Original Result: {0}", JsonSerializer.Serialize(originalResult.Matches.Select(x => x.Value)));
		TestConsole.WriteLine("New Result:      {0}", JsonSerializer.Serialize(newResult.Matches.Select(x => x.Value)));

		if (originalResult.Matches.Count != newResult.Matches.Count)
			Assert.Inconclusive("Stringified semantics do not match original string");

		foreach (var (o, n) in originalResult.Matches.Zip(newResult.Matches, (x, y) => (x, y)))
		{
			if (!o.Value.IsEquivalentTo(n.Value))
				Assert.Inconclusive("Stringified semantics do not match original string");
		}
	}

	[TestCase("$[?@ in [42, 43, 44]]", "[1, 2, 43, 54, 69]")]
	[TestCase("$[?@ + 6 == 0]", "[1, -2, \"-6\", -6, 69]")]
	[TestCase("$[?(@ + 6) * 2 == 0]", "[1, -2, \"-12\", -12, 69]")]
	public void InExpression(string pathText, string dataText)
	{
		var data = JsonNode.Parse(dataText);

		AssertStringify(pathText, data, _options);
	}
}