using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Json.More;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Json.Patch.Tests.Suite
{
	[TestFixture]
	public class JsonPatchTestSuite
	{
		private const string _testFolder = @"../../../../ref-repos/json-patch-tests";
		private static readonly JsonSerializerOptions _options;

		// ReSharper disable once MemberCanBePrivate.Global
		public static IEnumerable TestData => LoadTests();

		private static IEnumerable<TestCaseData> LoadTests()
		{
			var testsPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, _testFolder);
			var fileNames = Directory.GetFiles(testsPath, "*tests*.json");

			foreach (var fileName in fileNames)
			{
				var contents = File.ReadAllText(fileName);
				var suite = JsonSerializer.Deserialize<List<JsonPatchTest>>(contents, _options)!;

				foreach (var test in suite.Where(t => t != null))
				{
					var testName = test.Comment?.Replace(' ', '_') ?? "Unnamed test";
					yield return new TestCaseData(fileName, test) {TestName = testName};
				}
			}
		}

		static JsonPatchTestSuite()
		{
			_options = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				WriteIndented = true
			};
		}

		[TestCaseSource(nameof(TestData))]
		public void Run(string fileName, JsonPatchTest test)
		{
			var isOptional = test.Disabled;

			PatchResult? result = null;
			using (new TestExecutionContext.IsolatedContext())
			{
				try
				{
					result = test.Patch.Apply(test.Doc);

					Assert.AreNotEqual(test.ExpectsError, result.IsSuccess);

					if (test.HasExpectedValue)
						Assert.IsTrue(test.ExpectedValue.IsEquivalentTo(result.Result));
				}
				catch (Exception e)
				{
					Console.WriteLine();
					Console.WriteLine(fileName);
					Console.WriteLine(JsonSerializer.Serialize(test, _options));
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
					if (result != null)
					{
						if (result.Result.ValueKind != JsonValueKind.Undefined)
							Console.WriteLine(result.Result.GetRawText());
						Console.WriteLine(result.Error);
					}
					if (isOptional)
						Assert.Inconclusive("This is an acceptable failure.  Test case failed, but was marked as 'disabled'.");
					throw;
				}
			}
		}
	}
}