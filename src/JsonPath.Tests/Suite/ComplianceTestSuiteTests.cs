using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using NUnit.Framework;
using TestHelpers;

namespace Json.Path.Tests.Suite;

public class ComplianceTestSuiteTests
{
	private const string _testsFile = @"../../../../../ref-repos/jsonpath-compliance-test-suite/cts.json";
	private static readonly string[] _notSupported = [];

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

		TestConsole.WriteLine();
		TestConsole.WriteLine();
		TestConsole.WriteLine(testCase);
		TestConsole.WriteLine();

		if (testCase.InvalidSelector)
		{
			bool tryParseResult;
			try
			{
				tryParseResult = JsonPath.TryParse(testCase.Selector, out _);
			}
			catch (Exception e)
			{
				TestConsole.WriteLine(e);
				Assert.Fail("TryParse() threw an exception");
				throw; // this will never run, but the compiler doesn't know that Assert.Fail() will always throw.
			}
			Assert.That(tryParseResult, Is.False);

			var exception = Assert.Throws<PathParseException>(() => JsonPath.Parse(testCase.Selector));
			TestConsole.WriteLine($"Error: {exception!.Message}");
			return;
		}
		
		var path = JsonPath.Parse(testCase.Selector);
		Evaluate(path, testCase);

		var success = JsonPath.TryParse(testCase.Selector, out path);
		Assert.That(success, Is.True);
		Evaluate(path!, testCase);
	}

	private static void Evaluate(JsonPath path, ComplianceTestCase testCase)
	{
		var actual = path.Evaluate(testCase.Document);

		var actualValues = actual.Matches.Select(m => m.Value).ToJsonArray();
		var actualLocations = actual.Matches.Select(m => (JsonValue) m.Location!.ToString()).ToJsonArray();
		TestConsole.WriteLine($"Actual (values): {JsonSerializer.Serialize(actualValues, SerializerOptions.Default)}");
		TestConsole.WriteLine($"Actual (locations): {JsonSerializer.Serialize(actualLocations, SerializerOptions.Default)}");
		TestConsole.WriteLine();
		TestConsole.WriteLine($"Actual: {JsonSerializer.Serialize(actual, SerializerOptions.Default)}");
		if (testCase.InvalidSelector)
			Assert.Fail($"{testCase.Selector} is not a valid path.");

		if (testCase.Result is not null)
		{
			Assert.That(testCase.Result.IsEquivalentTo(actualValues), Is.True, "Unexpected results returned");
			Assert.That(testCase.Location.IsEquivalentTo(actualLocations), Is.True, "Unexpected results returned");
		}
		else
		{
			Assert.That(() => testCase.Results!.Contains(actualValues, JsonNodeEqualityComparer.Instance), "None of the options matched.");
			Assert.That(() => testCase.Locations!.Contains(actualLocations, JsonNodeEqualityComparer.Instance), "None of the options matched.");
		}
	}
}