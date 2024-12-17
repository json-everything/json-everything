using System.Collections.Generic;
using NUnit.Framework;
using TestHelpers;

namespace Json.Path.Tests;

public class ParsingTests
{
	public static IEnumerable<TestCaseData> SuccessCases =>
		[
			new("$['foo']"),
			new("$[ 'foo']"),
			new("$['foo' ]"),

			new("$[1]"),
			new("$[ 1]"),
			new("$[1 ]"),
			new("$[42]"),
			new("$[-1]"),
			new("$[-42]"),

			new("$[*]"),
			new("$[ *]"),
			new("$[* ]"),

			new("$[:]"),
			new("$[1:]"),
			new("$[1::]"),
			new("$[1:2]"),
			new("$[1:2:]"),
			new("$[1::3]"),
			new("$[1:2:3]"),
			new("$[-1:2:3]"),
			new("$[1:-2:3]"),
			new("$[1:2:-3]"),
			new("$[ 1:2:3]"),
			new("$[1 :2:3]"),
			new("$[1: 2:3]"),
			new("$[1:2 :3]"),
			new("$[1:2: 3]"),
			new("$[1:2:3 ]"),

			new("$.foo"),

			new("$.*"),

			new("$..foo"),
			new("$..*"),
			new("$..[1]"),
			new("$..[1,2]"),

			new("$[?@.foo]"),
			new("$[?(@.foo)]"),
			new("$[?(@.foo && @.bar)]"),
			new("$[?(@.foo && @.bar || @.baz)]"),
			new("$[?(@.foo || @.bar && @.baz)]"),
			new("$[?(!@.foo)]"),
			new("$[?(@.foo && !@.bar)]"),
			new("$[?!(@.foo == false)]"),
			new("$[?(@.foo == false)]"),
			new("$[?(@['name'] == null || @['name'] == 'abc')]"),

			new("$[1,'foo',1:2:3,*]"),
		];

	[TestCaseSource(nameof(SuccessCases))]
	public void ParseSuccess(string path)
	{
		TestConsole.WriteLine(JsonPath.Parse(path));
	}

	public static IEnumerable<TestCaseData> OptionalMathCases =>
		[
			new("$[?(@.foo==(4+5))]"),
			new("$[?(@.foo==2*(4+5))]"),
			new("$[?(@.foo==2+(4+5))]"),
			new("$[?(@.foo==2-(4+5))]"),
			new("$[?(@.foo==2*4+5)]"),
			new("$[?((4+5)==@.foo)]"),
			new("$[?(2*(4+5)==@.foo)]"),
			new("$[?(2+(4+5)==@.foo)]"),
			new("$[?(2-(4+5)==@.foo)]"),
			new("$[?(2*4+5==@.foo)]"),
			new("$[?@.foo==(4+5)]"),
			new("$[?@.foo==2*(4+5)]"),
			new("$[?@.foo==2+(4+5)]"),
			new("$[?@.foo==2-(4+5)]"),
			new("$[?@.foo==2*4+5]"),
			new("$[?(4+5)==@.foo]"),
			new("$[?2*(4+5)==@.foo]"),
			new("$[?2+(4+5)==@.foo]"),
			new("$[?2-(4+5)==@.foo]"),
			new("$[?2*4+5==@.foo]"),
		];

	[TestCaseSource(nameof(OptionalMathCases))]
	public void ParseMathWithOptions(string path)
	{
		TestConsole.WriteLine(JsonPath.Parse(path, new PathParsingOptions{AllowMathOperations = true}));
	}

	[TestCaseSource(nameof(OptionalMathCases))]
	public void ParseMathWithoutOptions(string path)
	{
		Assert.Throws<PathParseException>(() => JsonPath.Parse(path));
	}

	public static IEnumerable<TestCaseData> OptionalJsonLiteralCases =>
		[
			new("$[?@.foo==[1,2,3]]"),
			new("$[?@.foo=={\"bar\":\"object\"}]"),
		];

	[TestCaseSource(nameof(OptionalJsonLiteralCases))]
	public void ParseLiteralWithOptions(string path)
	{
		TestConsole.WriteLine(JsonPath.Parse(path, new PathParsingOptions{AllowJsonConstructs = true}));
	}

	[TestCaseSource(nameof(OptionalJsonLiteralCases))]
	public void ParseLiteralWithoutOptions(string path)
	{
		Assert.Throws<PathParseException>(() => JsonPath.Parse(path));
	}

	public static IEnumerable<TestCaseData> OptionalInOpCases =>
		[
			new("$[?5 in @.foo]"),
		];

	[TestCaseSource(nameof(OptionalInOpCases))]
	public void ParseInOpWithOptions(string path)
	{
		TestConsole.WriteLine(JsonPath.Parse(path, new PathParsingOptions{AllowInOperator = true}));
	}

	[TestCaseSource(nameof(OptionalInOpCases))]
	public void ParseInOpWithoutOptions(string path)
	{
		Assert.Throws<PathParseException>(() => JsonPath.Parse(path));
	}

	[TestCaseSource(nameof(SuccessCases))]
	public void ParseRelativeStarts(string path)
	{
		path = $"@{path.Substring(1)}"; // Turn the absolute path into a relative path
		Assert.DoesNotThrow(() => JsonPath.Parse(path, new PathParsingOptions{AllowRelativePathStart = true}));
	}

	[TestCaseSource(nameof(SuccessCases))]
	public void TryParseRelativeStarts(string path)
	{
		path = $"@{path.Substring(1)}"; // Turn the absolute path into a relative path
		Assert.That(JsonPath.TryParse(path, out _, new PathParsingOptions{AllowRelativePathStart = true}));
	}
}