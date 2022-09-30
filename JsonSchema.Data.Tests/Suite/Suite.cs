using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Humanizer;
using Json.More;
using Json.Schema.Tests;
using NUnit.Framework;

namespace Json.Schema.Data.Tests.Suite;

public class Suite
{
	private const string _testCasesPath = @"../../../../ref-repos/json-schema-vocab-test-suites/tests/data";
	private const string _remoteSchemasPath = @"../../../../ref-repos/json-schema-vocab-test-suites/tests/data/external-sources";

	private const bool _useExternal = false;
	private const string _externalTestCasesPath = @"../../../../../json-schema-vocab-test-suites/tests/data";
	private const string _externalRemoteSchemasPath = @"../../../../../json-schema-vocab-test-suites/tests/data/external-sources";

	public static IEnumerable<TestCaseData> TestCases()
	{
		// ReSharper disable once HeuristicUnreachableCode
		var testCasesPath = _useExternal ? _externalTestCasesPath : _testCasesPath;

		var testsPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, testCasesPath)
			.AdjustForPlatform();
		if (!Directory.Exists(testsPath)) return Enumerable.Empty<TestCaseData>();

		var fileNames = Directory.GetFiles(testsPath, "*.json");
		var options = new ValidationOptions
		{
			OutputFormat = OutputFormat.Verbose
		};

		var allTests = new List<TestCaseData>();
		foreach (var fileName in fileNames)
		{
			var shortFileName = Path.GetFileNameWithoutExtension(fileName);
			var contents = File.ReadAllText(fileName);
			var collections = JsonSerializer.Deserialize<List<TestCollection>>(contents, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			});

			foreach (var collection in collections!)
			{
				collection.IsOptional = fileName.Contains("optional");
				foreach (var test in collection.Tests)
				{
					var optional = collection.IsOptional ? "(optional)/" : null;
					var name = $"{shortFileName}/{optional}{collection.Description.Kebaberize()}/{test.Description.Kebaberize()}";
					var optionsCopy = ValidationOptions.From(options);
					allTests.Add(new TestCaseData(collection, test, shortFileName, optionsCopy) { TestName = name });
				}
			}
		}

		return allTests;
	}

	static Suite()
	{
		Vocabularies.Register();
	}

	[OneTimeSetUp]
	public static void LoadRemoteSchemas()
	{
		var remotesPath = _useExternal ? _externalRemoteSchemasPath : _remoteSchemasPath;

		var remotesFilePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, remotesPath)
			.AdjustForPlatform();
		if (!Directory.Exists(remotesFilePath))
			throw new Exception("cannot find remotes path");

		DataKeyword.Fetch = uri =>
		{
			var filePath = uri.OriginalString.Replace("http://localhost:1234", remotesPath);
			var text = File.ReadAllText(filePath);
			return JsonNode.Parse(text);
		};
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
		if (test.Error)
			Console.WriteLine("expected error");
		else
			Console.WriteLine(test.Valid ? "valid" : "invalid");
		Console.WriteLine();
		Console.WriteLine(JsonSerializer.Serialize(collection.Schema, serializerOptions));
		Console.WriteLine();
		Console.WriteLine(test.Data.AsJsonString());
		Console.WriteLine();

		if (!InstanceIsDeserializable(test.Data))
			Assert.Inconclusive("Instance not deserializable");

		if (test.Error)
		{
			Assert.Throws(Is.InstanceOf<Exception>(), () => collection.Schema.Validate(test.Data, options));
			return;
		}

		var result = collection.Schema.Validate(test.Data, options);
		//result.ToBasic();
		Console.WriteLine(JsonSerializer.Serialize(result, serializerOptions));

		if (collection.IsOptional && result.IsValid != test.Valid)
			Assert.Inconclusive("Test optional");
		Assert.AreEqual(test.Valid, result.IsValid);
	}

	private static bool InstanceIsDeserializable(in JsonNode? testData)
	{
		try
		{
			var value = (testData as JsonValue)?.GetValue<object>();
			if (value is null) return true;
			if (value is string) return false;
			if (value is JsonElement { ValueKind: JsonValueKind.Number } element)
				return element.TryGetDecimal(out _);
			if (value.GetType().IsNumber())
			{
				// some tests involve numbers larger than c# can handle.  fortunately, they're optional.
				return true;
			}

			return true;
		}
		catch (Exception e)
		{
			Console.WriteLine(e.Message);
			return false;
		}
	}

	[Test]
	public void EnsureTestSuiteConfiguredForServerBuild()
	{
		Assert.IsFalse(_useExternal);
	}
}