using System;
using System.Collections.Generic;
using NUnit.Framework;
using TestHelpers;

namespace Json.Path.Tests;

public class InternalJsonParsingTests
{
	[TestCaseSource(nameof(GetJson))]
	public static void Parse(string json)
	{
		int i = 0;
		if (!json.AsSpan().TryParseJson(ref i, out var node))
			Assert.Fail();

		TestConsole.WriteLine(node);
	}

	private static IEnumerable<string> GetJson()
	{
		return
		[
			"-42",
			"1+5",
			"true",
			"false",
			"true and some more",
			"false this is different",
			"5.6/7",
			"\"a string\"; then this",
			"[null, true]; and some more",
			"{\"key\": \"value\"}, then the end"
		];
	}
}