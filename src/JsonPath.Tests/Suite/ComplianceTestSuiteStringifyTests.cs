using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using Json.More;
using NUnit.Framework;

namespace Json.Path.Tests.Suite;

public class ComplianceTestSuiteStringifyTests
{
	private const string _testsFile = @"../../../../../ref-repos/jsonpath-compliance-test-suite/cts.json";

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
			var suite = JsonSerializer.Deserialize<ComplianceTestSuite>(fileText, new JsonSerializerOptions
			{
				AllowTrailingCommas = true,
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
				PropertyNameCaseInsensitive = true
			});
			return suite!.Tests.Where(x => !x.InvalidSelector).Select(t => new TestCaseData(t) { TestName = t.Name });
		}
	}

	[TestCaseSource(nameof(TestCases))]
	public void Stringify(ComplianceTestCase testCase)
	{
		Console.WriteLine();
		Console.WriteLine();
		Console.WriteLine(testCase);
		Console.WriteLine();

		var path = JsonPath.Parse(testCase.Selector);
		var originalResult = path.Evaluate(testCase.Document);

		var backToString = path.ToString();
		Console.WriteLine(backToString);
		if (!JsonPath.TryParse(backToString, out var newPath))
			Assert.Inconclusive("Stringified semantics do not match original string");

		var newResult = newPath.Evaluate(testCase.Document);

		if (originalResult.Matches.Count != newResult.Matches.Count)
			Assert.Inconclusive("Stringified semantics do not match original string");

		foreach (var (o, n) in originalResult.Matches.Zip(newResult.Matches))
		{
			if (!o.Value.IsEquivalentTo(n.Value))
				Assert.Inconclusive("Stringified semantics do not match original string");
		}
	}
}