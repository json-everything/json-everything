using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using NUnit.Framework;
using TestHelpers;

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
		TestConsole.WriteLine();
		TestConsole.WriteLine();
		TestConsole.WriteLine(testCase);
		TestConsole.WriteLine();

		StringifyTests.AssertStringify(testCase.Selector, testCase.Document);
	}
}