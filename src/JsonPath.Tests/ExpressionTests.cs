using System.Linq;
using System.Text.Json.Nodes;
using Json.More;
using NUnit.Framework;

namespace Json.Path.Tests;

public class ExpressionTests
{
	[TestCase("$[?(@.foo==(4+5))]", "[{\"foo\": 9}]")]
	[TestCase("$[?(@.foo==2*(4+5))]", "[{\"foo\": 18}]")]
	[TestCase("$[?(@.foo==2+(4+5))]", "[{\"foo\": 11}]")]
	[TestCase("$[?(@.foo==2-(4+5))]", "[{\"foo\": -7}]")]
	[TestCase("$[?(@.foo==2*4+5)]", "[{\"foo\": 13}]")]
	[TestCase("$[?(@.foo==2+4*5)]", "[{\"foo\": 22}]")]
	public void OrderOfOperations(string pathString, string expectedString)
	{
		var target = JsonNode.Parse(
			"""
			[
			  {"foo": 9},
			  {"foo": 18},
			  {"foo": 11},
			  {"foo": -7},
			  {"foo": 13},
			  {"foo": 22}
			]
			""");

		var path = JsonPath.Parse(pathString, new PathParsingOptions { AllowMathOperations = true });
		var expected = JsonNode.Parse(expectedString);

		var actual = path.Evaluate(target).Matches!.Select(x => x.Value).ToJsonArray();

		Assert.IsTrue(actual.IsEquivalentTo(expected));
	}
}