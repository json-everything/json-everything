using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Json.More;
using NUnit.Framework;

namespace Json.Path.Tests.Suite;

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
		// dashes are not allowed in shorthand property names
		"$.key-dash",
		"$[?(@.key-dash == 'value')]",

		// shorthand property names must start with a letter
		"$.2",
		"$[?(@.2 == 'second')]",
		"$[?(@.2 == 'third')]",
		"$.-1",
		"$.$",
		"$.'key'",
		"$.\"key\"",
		"$..\"key\"",
		"$..'key'",
		"$..'key'",
		"$.'some.key'",

		// leading zeroes are not allowed for numeric literals
		"$[?(@.key==010)]",
		"$[010:024:010]",

		// JSON literals are not expression results
		"$[?(@.key>0 && false)]",
		"$[?(@.key>0 && true)]",
		"$[?(@.key>0 || false)]",
		"$[?(@.key>0 || true)]",
		"$[?((@.key<44)==false)]",
		"$[?(true)]",
		"$[?(false)]",
		"$[?(null)]",

		// trailing whitespace is disallowed
		"$. a ",

		// regex operator transitioned to functions
		"$[?(@.name=~/hello.*/)]",
		"$[?(@.name=~/@.pattern/)]",

		// functions are only valid in expressions
		// and are not extensions on paths
		"$.data.sum()",
		"$[?(@.length() == 4)]",

		// relative paths were excluded
		"@.a",

		// 'in' operator was excluded
		"$[?(@.d in [2, 3])]",
		"$[?(2 in @.d)]",

		// only literals are supported in expressions
		"$[?(@.d==['v1','v2'])]",

		// other invalid syntaxes
		"$...key",
		"$.['key']",
		"$.[\"key\"]",
		"$..",
		"$.key..",
		".key",
		"key",
		"",
		"$[?(@.key===42)]",

		// big numbers not supported
		"$[2:-113667776004:-1]",
		"$[113667776004:2:-1]",
	};

	private static readonly JsonSerializerOptions _serializerOptions = new()
	{
		WriteIndented = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};
	private static readonly JsonSerializerOptions _linearSerializerOptions = new()
	{
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
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
			static bool TryMatch(string line, Regex pattern, [NotNullWhen(true)] out string? value)
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
			CburgmerTestCase? currentTestCase = null;
			foreach (var line in fileLines)
			{
				if (TryMatch(line, _idPattern, out var value))
				{
					if (currentTestCase != null)
						yield return new TestCaseData(currentTestCase) { TestName = currentTestCase.TestName };
					currentTestCase = new CburgmerTestCase { TestName = value };
				}
				else if (TryMatch(line, _selectorPattern, out value))
				{
					currentTestCase!.PathString = JsonNode.Parse(value)!.GetValue<string>();
				}
				else if (TryMatch(line, _documentPattern, out value))
				{
					currentTestCase!.JsonString = value;
				}
				else if (TryMatch(line, _consensusPattern, out value))
				{
					currentTestCase!.Consensus = value;
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

		PathResult? actual = null;

		Exception? exception = null;
		var time = Debugger.IsAttached ? int.MaxValue : 100;
		using var cts = new CancellationTokenSource(time);
		Task.Run(() => actual = Evaluate(testCase.JsonString, testCase.PathString), cts.Token)
			.ContinueWith(taskResult =>
			{
				if (taskResult.IsFaulted)
					exception = taskResult.Exception!.InnerException;
			}, cts.Token)
			.Wait(cts.Token);

		if (actual == null)
		{
			if (testCase.Consensus == "NOT_SUPPORTED")
			{
				if (exception != null) 
					Console.WriteLine(exception);
				return;
			}

			if (exception != null) throw exception;

			if (testCase.Consensus == null)
				Assert.Inconclusive("Test case has no consensus result.  Cannot validate.");

			Assert.Fail($"Could not parse path: {testCase.PathString}");
		}

		Console.WriteLine($"Actual (values): {JsonSerializer.Serialize(actual!.Matches!.Select(x => x.Value), _linearSerializerOptions)}");
		Console.WriteLine();
		Console.WriteLine($"Actual: {JsonSerializer.Serialize(actual, _serializerOptions)}");
		if (testCase.Consensus == null)
			Assert.Inconclusive("Test case has no consensus result.  Cannot validate.");
		else
		{
			if (testCase.Consensus == "NOT_SUPPORTED") return;
			var expected = JsonNode.Parse(testCase.Consensus);
			Assert.IsTrue(expected!.AsArray().All(v => actual.Matches!.Any(m => JsonNodeEqualityComparer.Instance.Equals(v, m.Value))));
		}
	}

	private static PathResult? Evaluate(string jsonString, string pathString)
	{
		var o = JsonNode.Parse(jsonString);

		var path = JsonPath.Parse(pathString);
		var results = path?.Evaluate(o);

		return results;
	}
}