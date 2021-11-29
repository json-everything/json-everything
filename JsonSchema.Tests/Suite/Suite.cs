using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using Humanizer;
using Json.More;
using NUnit.Framework;

namespace Json.Schema.Tests.Suite
{
	public class Suite
	{
		private const string _testCasesPath = @"../../../../ref-repos/JSON-Schema-Test-Suite/tests";
		private const string _remoteSchemasPath = @"../../../../ref-repos/JSON-Schema-Test-Suite/remotes";

		public static IEnumerable<TestCaseData> TestCases()
		{
			return GetTests("draft6")
				.Concat(GetTests("draft7"))
				.Concat(GetTests("draft2019-09"))
				.Concat(GetTests("draft2020-12"));
		}

		private static IEnumerable<TestCaseData> GetTests(string draftFolder)
		{
			var testsPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, _testCasesPath, $"{draftFolder}/")
				.AdjustForPlatform();
			if (!Directory.Exists(testsPath)) return Enumerable.Empty<TestCaseData>();

			var fileNames = Directory.GetFiles(testsPath, "*.json", SearchOption.AllDirectories);
			var options = new ValidationOptions
			{
				OutputFormat = OutputFormat.Verbose
			};
			switch (draftFolder)
			{
				case "draft6":
					options.ValidateAs = Draft.Draft6;
					break;
				case "draft7":
					options.ValidateAs = Draft.Draft7;
					break;
				case "draft2019-09":
					options.ValidateAs = Draft.Draft201909;
					break;
				case "draft2020-12":
					// will set this when implementing the next draft
					//options.ValidateAs = Draft.Draft202012;
					break;
			}

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
						allTests.Add(new TestCaseData(collection, test, shortFileName, options) { TestName = name });
					}
				}
			}

			return allTests;
		}

		[OneTimeSetUp]
		public void LoadRemoteSchemas()
		{
			var remotesPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, _remoteSchemasPath)
				.AdjustForPlatform();
			if (!Directory.Exists(remotesPath)) throw new Exception("Cannot find remotes folder");

			var fileNames = Directory.GetFiles(remotesPath, "*.json", SearchOption.AllDirectories);

			foreach (var fileName in fileNames)
			{
				var schema = JsonSchema.FromFile(fileName);
				var uri = new Uri(fileName.Replace(remotesPath, "http://localhost:1234").Replace('\\', '/'));
				SchemaRegistry.Global.Register(uri, schema);
			}
		}

		[TestCaseSource(nameof(TestCases))]
		public void Test(TestCollection collection, TestCase test, string fileName, ValidationOptions options)
		{
			var serializerOptions = new JsonSerializerOptions
			{
				WriteIndented = true,
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			};

			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine(fileName);
			Console.WriteLine(collection.Description);
			Console.WriteLine(test.Description);
			Console.WriteLine(test.Valid ? "valid" : "invalid");
			Console.WriteLine();
			Console.WriteLine(JsonSerializer.Serialize(collection.Schema, serializerOptions));
			Console.WriteLine();
			Console.WriteLine(test.Data.ToJsonString());
			Console.WriteLine();

			if (!InstanceIsDeserializable(test.Data))
				Assert.Inconclusive("Instance not deserializable");

			var result = collection.Schema.Validate(test.Data, options);
			result.ToDetailed();
			Console.WriteLine(JsonSerializer.Serialize(result, serializerOptions));

			if (collection.IsOptional && result.IsValid != test.Valid)
				Assert.Inconclusive("Test optional");
			Assert.AreEqual(test.Valid, result.IsValid);
		}

		//[TestCaseSource(nameof(TestCases))]
		// This is for local runs only.
		public void Benchmark(TestCollection collection, TestCase test, string fileName, ValidationOptions options)
		{
			if (!InstanceIsDeserializable(test.Data))
				Assert.Inconclusive("Instance not deserializable");

			options.OutputFormat = OutputFormat.Flag;
			var result = collection.Schema.Validate(test.Data, options);

			if (collection.IsOptional && result?.IsValid != test.Valid)
				Assert.Inconclusive("Test optional");
			Assert.AreEqual(test.Valid, result.IsValid);
		}

		private static bool InstanceIsDeserializable(in JsonElement testData)
		{
			try
			{
				switch (testData.ValueKind)
				{
					case JsonValueKind.Undefined:
						return false;
					case JsonValueKind.Number:
						// some tests involve numbers larger than c# can handle.  fortunately, they're optional.
						testData.GetDecimal();
						return true;
					default:
						return true;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return false;
			}
		}
	}
}