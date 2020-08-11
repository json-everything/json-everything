using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Humanizer;
using NUnit.Framework;

namespace Json.Schema.Tests.Suite
{
	public class Suite
	{
		private const string _testCasesPath = @"../../../../ref-repos/JSON-Schema-Test-Suite/tests";

		public static IEnumerable<TestCaseData> TestCases()
		{
			return GetTests("draft6")
				.Concat(GetTests("draft7"))
				.Concat(GetTests("draft2019-09"));
		}

		private static IEnumerable<TestCaseData> GetTests(string draftFolder)
		{
			var testsPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, _testCasesPath, $"{draftFolder}/")
				.AdjustForPlatform();
			if (!Directory.Exists(testsPath)) return Enumerable.Empty<TestCaseData>();

			var fileNames = Directory.GetFiles(testsPath, "*.json", SearchOption.AllDirectories);

			var allTests = new List<TestCaseData>();
			foreach (var fileName in fileNames)
			{
				var shortFileName = Path.GetFileNameWithoutExtension(fileName);
				var contents = File.ReadAllText(fileName);
				var collections = JsonSerializer.Deserialize<List<TestCollection>>(contents, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				foreach (var collection in collections)
				{
					collection.IsOptional = fileName.Contains("optional");
					foreach (var test in collection.Tests)
					{
						var optional = collection.IsOptional ? "(optional)/" : null;
						var name = $"{draftFolder}/{optional}{collection.Description.Kebaberize()}/{test.Description.Kebaberize()}";
						allTests.Add(new TestCaseData(collection, test, shortFileName) { TestName = name });
					}
				}
			}

			return allTests;
		}

		[TestCaseSource(nameof(TestCases))]
		public void Test(TestCollection collection, TestCase test, string fileName)
		{
			var result = collection.Schema.Validate(test.Data);

			if (result.IsValid == test.Valid) return;
			if (collection.IsOptional)
				Assert.Inconclusive("Test optional");
			
			Console.WriteLine(fileName);
			Console.WriteLine(collection.Description);
			Console.WriteLine(test.Description);
			Console.WriteLine();
			Console.WriteLine(JsonSerializer.Serialize(collection.Schema, new JsonSerializerOptions{WriteIndented = true}));
			Console.WriteLine();
			Console.WriteLine(test.Data);

			Assert.AreEqual(test.Valid, result.IsValid);
		}
	}
}