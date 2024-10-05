using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Json.More;
using NUnit.Framework;
using NUnit.Framework.Internal;
using TestHelpers;

namespace Json.Patch.Tests.Suite;

[TestFixture]
public class JsonPatchTestSuite
{
	private const string _testFolder = "../../../../../ref-repos/json-patch-tests";

	// ReSharper disable once MemberCanBePrivate.Global
	public static IEnumerable TestData => LoadTests();

	private static IEnumerable<TestCaseData> LoadTests()
	{
		var testsPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, _testFolder);
		var fileNames = Directory.GetFiles(testsPath, "*tests*.json");

		foreach (var fileName in fileNames)
		{
			var contents = File.ReadAllText(fileName);
			var suite = JsonSerializer.Deserialize<JsonPatchTest[]>(contents, TestEnvironment.SerializerOptions)!;

			foreach (var test in suite.Where(t => t != null!))
			{
				var testName = test.Comment ?? "Unnamed test";
				yield return new TestCaseData(fileName, test) { TestName = testName };
			}
		}
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
				result = test.Patch!.Apply(test.Doc);

				Assert.That(result.IsSuccess, Is.Not.EqualTo(test.ExpectsError));

				if (test.HasExpectedValue)
					Assert.That(test.ExpectedValue.IsEquivalentTo(result.Result), Is.True);
			}
			catch (Exception e)
			{
				TestConsole.WriteLine();
				TestConsole.WriteLine(fileName);
				TestConsole.WriteLine(JsonSerializer.Serialize(test, TestEnvironment.SerializerOptions));
				TestConsole.WriteLine(e.Message);
				TestConsole.WriteLine(e.StackTrace);
				if (result != null)
				{
					TestConsole.WriteLine(result.Result.AsJsonString(TestEnvironment.SerializerOptions));
					TestConsole.WriteLine(result.Error);
				}
				if (isOptional)
					Assert.Inconclusive("This is an acceptable failure.  Test case failed, but was marked as 'disabled'.");
				throw;
			}
		}
	}
}