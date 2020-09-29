using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Json.More;
using Json.Path;
using NUnit.Framework;

namespace JsonPath.Tests.Suite
{
	public class ReferenceImplTests
	{
		private const string _testsFile = @"..\..\..\..\ref-repos\jsonpath-reference-implementation\tests\cts.json";
		private static readonly string[] _notSupported =
			{
				// expect these to be out of spec soon
				"$.key-dash",
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
				var fileText = File.ReadAllText(_testsFile);
				var suite = JsonSerializer.Deserialize<ReferenceImplTestSuite>(fileText, new JsonSerializerOptions
				{
					AllowTrailingCommas = true,
					Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
					PropertyNameCaseInsensitive = true
				});
				return suite.Tests.Select(t => new TestCaseData(t) {TestName = t.Name});
			}
		}

		[TestCaseSource(nameof(TestCases))]
		public void Run(ReferenceImplTestCase testCase)
		{
			if (_notSupported.Contains(testCase.Selector))
				Assert.Inconclusive("This case will not be supported.");

			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine(testCase);
			Console.WriteLine();

			PathResult actual = null;

			var time = Debugger.IsAttached ? int.MaxValue : 100;
			using var cts = new CancellationTokenSource(time);
			Task.Run(() => actual = Evaluate(testCase.Document, testCase.Selector), cts.Token).Wait(cts.Token);

			if (actual == null)
			{
				if (testCase.InvalidSelector) return;
				Assert.Fail($"Could not parse path: {testCase.Selector}");
			}

			Console.WriteLine($"Actual: {JsonSerializer.Serialize(actual)}");
			if (testCase.InvalidSelector)
				Assert.Fail($"{testCase.Selector} is not a valid path.");

			var expected = testCase.Result;
			Assert.IsTrue(expected.Select((v, i) => (v, i)).All(v => JsonElementEqualityComparer.Instance.Equals(v.v, TryGetValueAtIndex(actual.Matches, v.i)?.Value ?? default)));
		}

		private static T TryGetValueAtIndex<T>(IReadOnlyList<T> collection, int i)
		{
			if (0 <= i && i < collection.Count) return collection[i];
			return default;
		}

		private static PathResult Evaluate(JsonElement element, string pathString)
		{
			var selector = pathString;
			if (!Json.Path.JsonPath.TryParse(selector, out var path))
				return null;
			var results = path.Evaluate(element);

			return results;
		}
	}
}
