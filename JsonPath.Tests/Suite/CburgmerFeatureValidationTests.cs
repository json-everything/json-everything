using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Json.More;
using NUnit.Framework;

namespace Json.Path.Tests.Suite
{
	/// <summary>
	/// These are a set of tests that GitHub user cburgmer uses to check all JSON Path implementations
	/// for feature support.  Adding the suite here ensures that I support them all.
	/// </summary>
	/// <remarks>This is from cburgmer's amazing reporting site: https://cburgmer.github.io/json-path-comparison/</remarks>
	public class CburgmerFeatureValidationTests
	{
		private const string _regressionResultsFile = @"../../../../ref-repos/json-path-comparison/regression_suite/regression_suite.yaml";
		private static readonly Regex _idPattern = new Regex(@"  - id: (?<value>.*)");
		private static readonly Regex _selectorPattern = new Regex(@"    selector: (?<value>.*)");
		private static readonly Regex _documentPattern = new Regex(@"    document: (?<value>.*)");
		private static readonly Regex _consensusPattern = new Regex(@"    consensus: (?<value>.*)");
		private static readonly string[] _notSupported =
			{
				// expect these to be out of spec soon
				"$.key-dash",
				"$.-1",
				"$.length",

				// big numbers not supported
				"$[2:-113667776004:-1]",
				"$[113667776004:2:-1]"
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
				static bool TryMatch(string line, Regex pattern, out string value)
				{
					var match = pattern.Match(line);
					if (!match.Success)
					{
						value = null;
						return false;
					}

					value = match.Groups["value"].Value;
					return true;
				}

				// what I wouldn't give for a YAML parser...
				var fileLines = File.ReadAllLines(_regressionResultsFile);
				CburgmerTestCase currentTestCase = null;
				foreach (var line in fileLines)
				{
					if (TryMatch(line, _idPattern, out var value))
					{
						if (currentTestCase != null)
							yield return new TestCaseData(currentTestCase) {TestName = currentTestCase.TestName};
						currentTestCase = new CburgmerTestCase{TestName = value};
					}
					else if (TryMatch(line, _selectorPattern, out value))
					{
						currentTestCase.PathString = JsonDocument.Parse(value).RootElement.GetString();
					}
					else if (TryMatch(line, _documentPattern, out value))
					{
						currentTestCase.JsonString = value;
					}
					else if (TryMatch(line, _consensusPattern, out value))
					{
						currentTestCase.Consensus = value;
					}
				}
			}
		}

		[TestCaseSource(nameof(TestCases))]
		public void Run(CburgmerTestCase testCase)
		{
			if (_notSupported.Contains(testCase.PathString))
				Assert.Inconclusive("This case will not be supported.");

			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine(testCase);
			Console.WriteLine();

			PathResult actual = null;

			var time = Debugger.IsAttached ? int.MaxValue : 100;
			using var cts = new CancellationTokenSource(time);
			Task.Run(() => actual = Evaluate(testCase.JsonString, testCase.PathString), cts.Token).Wait(cts.Token);

			if (actual == null)
			{
				if (testCase.Consensus == "NOT_SUPPORTED") return;
				if (testCase.Consensus == null)
					Assert.Inconclusive("Test case has no consensus result.  Cannot validate.");

				Assert.Fail($"Could not parse path: {testCase.PathString}");
			}

			Console.WriteLine($"Actual (values): {JsonSerializer.Serialize(actual.Matches.Select(x => x.Value))}");
			Console.WriteLine();
			Console.WriteLine($"Actual: {JsonSerializer.Serialize(actual)}");
			if (testCase.Consensus == null)
				Assert.Inconclusive("Test case has no consensus result.  Cannot validate.");
			else
			{
				if (testCase.Consensus == "NOT_SUPPORTED") return;
				var expected = JsonDocument.Parse(testCase.Consensus).RootElement;
				Assert.IsTrue(expected.EnumerateArray().All(v => actual.Matches.Any(m => JsonElementEqualityComparer.Instance.Equals(v, m.Value))));
			}
		}

		private static PathResult Evaluate(string jsonString, string pathString)
		{
			var o = JsonDocument.Parse(jsonString).RootElement;
			var selector = pathString;
			if (!JsonPath.TryParse(selector, out var path))
				return null;
			var results = path.Evaluate(o);

			return results;
		}
	}
}
