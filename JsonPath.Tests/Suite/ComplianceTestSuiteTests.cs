using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using Json.More;
using NUnit.Framework;

namespace Json.Path.Tests.Suite;

public class ComplianceTestSuiteTests
{
	private const string _testsFile = @"../../../../ref-repos/jsonpath-compliance-test-suite/cts.json";
	private static readonly string[] _notSupported =
	{
	};

	//  - id: array_index
	//    pathSegment: $[2]
	//    document: ["first", "second", "third", "forth", "fifth"]
	//    consensus: ["third"]
	//    scalar-consensus: "third"
	public static IEnumerable<TestCaseData> TestCases
	{
		get
		{
			var fileText = File.ReadAllText(_testsFile);
			var suite = JsonSerializer.Deserialize<ComplianceTestSuite>(fileText, SerializerOptions.Default);
			return suite!.Tests.Select(t => new TestCaseData(t) { TestName = t.Name });
		}
	}

	[TestCaseSource(nameof(TestCases))]
	public void Run(ComplianceTestCase testCase)
	{
		if (_notSupported.Contains(testCase.Name))
			Assert.Inconclusive("This case will not be supported.");

		Console.WriteLine();
		Console.WriteLine();
		Console.WriteLine(testCase);
		Console.WriteLine();

		if (testCase.InvalidSelector)
		{
			bool tryParseResult;
			try
			{
				tryParseResult = JsonPath.TryParse(testCase.Selector, out _);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				Assert.Fail("TryParse() threw an exception");
				throw; // this will never run, but the compiler doesn't know that Assert.Fail() will always throw.
			}
			Assert.IsFalse(tryParseResult);

			var exception = Assert.Throws<PathParseException>(() => JsonPath.Parse(testCase.Selector));
			Console.WriteLine($"Error: {exception!.Message}");
			return;
		}

		var path = JsonPath.Parse(testCase.Selector);
		var actual = path.Evaluate(testCase.Document);

		var actualValues = actual.Matches!.Select(m => m.Value).ToJsonArray();
		Console.WriteLine($"Actual (values): {JsonSerializer.Serialize(actualValues, SerializerOptions.Default)}");
		Console.WriteLine();
		Console.WriteLine($"Actual: {JsonSerializer.Serialize(actual, SerializerOptions.Default)}");
		if (testCase.InvalidSelector)
			Assert.Fail($"{testCase.Selector} is not a valid path.");

		var expected = testCase.Result!.ToJsonArray();
		Assert.IsTrue(expected.IsEquivalentTo(actualValues), "Unexpected results returned");
	}
}

public static class SerializerOptions
{
	public static JsonSerializerOptions Default = new()
	{
		AllowTrailingCommas = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		PropertyNameCaseInsensitive = true
	};
}