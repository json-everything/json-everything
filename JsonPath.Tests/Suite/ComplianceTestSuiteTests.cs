using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Json.More;
using NUnit.Framework;

namespace Json.Path.Tests.Suite
{
	public class ComplianceTestSuiteTests
	{
		private const string _testsFile = @"../../../../ref-repos/jsonpath-compliance-test-suite/cts.json";
		private static readonly string[] _notSupported =
			{
			};

		//  - id: array_index
		//    selector: $[2]
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
				return suite.Tests.Select(t => new TestCaseData(t) {TestName = t.Name});
			}
		}

		[TestCaseSource(nameof(TestCases))]
		public void Run(ComplianceTestCase testCase)
		{
			if (_notSupported.Contains(testCase.Selector))
				Assert.Inconclusive("This case will not be supported.");

			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine(testCase);
			Console.WriteLine();

			JsonPath path = null;
			PathResult actual = null;

			var time = Debugger.IsAttached ? int.MaxValue : 100;
			using var cts = new CancellationTokenSource(time);
			Task.Run(() =>
			{
				if (!JsonPath.TryParse(testCase.Selector, out path)) return;

				if (testCase.Document.ValueKind == JsonValueKind.Undefined) return;

				actual = path.Evaluate(testCase.Document);
			}, cts.Token).Wait(cts.Token);

			if (path != null && testCase.InvalidSelector)
				Assert.Inconclusive($"{testCase.Selector} is not a valid path but was parsed without error.");

			if (actual == null)
			{
				if (testCase.InvalidSelector) return;
				Assert.Fail($"Could not parse path: {testCase.Selector}");
			}

			var actualValues = actual.Matches.Select(m => m.Value).AsJsonElement();
			Console.WriteLine($"Actual (values): {actualValues}");
			Console.WriteLine();
			Console.WriteLine($"Actual: {JsonSerializer.Serialize(actual)}");
			if (testCase.InvalidSelector)
				Assert.Fail($"{testCase.Selector} is not a valid path.");

			var expected = testCase.Result.AsJsonElement();
			Assert.IsTrue(expected.IsEquivalentTo(actualValues));
		}
	}
}
