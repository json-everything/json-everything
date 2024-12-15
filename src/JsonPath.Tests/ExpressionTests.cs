using System.Linq;
using System.Text.Json.Nodes;
using Json.More;
using NUnit.Framework;
using TestHelpers;

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

		var actual = path.Evaluate(target).Matches.Select(x => x.Value).ToJsonArray();

		JsonAssert.AreEquivalent(expected, actual);
	}

	public class NoOpFunction : NodelistFunctionDefinition
	{
		public override string Name => "noop";

		public NodeList? Evaluate(NodeList? nodeList)
		{
			return nodeList;
		}

	}

	[Test]
	public void ExpressionWithNoOpFunctionWorks()
	{
		FunctionRepository.RegisterNodelistFunction<NoOpFunction>();

		// should do the same thing as $[?@.*]
		var path = JsonPath.Parse("$[?noop(noop(@.*))]");
		var data = JsonNode.Parse(
			"""
			[
			  {"foo": 9},
			  {"foo": 18, "bar": false},
			  {},
			  42,
			  ["yes", "no", 0],
			  true,
			  null
			]
			""");

		var expected = JsonNode.Parse(
			"""
			[
			  {"foo": 9},
			  {"foo": 18, "bar": false},
			  ["yes", "no", 0]
			]
			""");

		var actual = path.Evaluate(data).Matches.Select(x => x.Value).ToJsonArray();

		JsonAssert.AreEquivalent(expected, actual);
	}
}
