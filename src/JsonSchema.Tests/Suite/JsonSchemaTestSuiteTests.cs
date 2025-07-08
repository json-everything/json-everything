using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.Tests.Suite;

public class JsonSchemaTestSuiteTests
{
    private const string _testCasesPath = @"../../../../../ref-repos/JSON-Schema-Test-Suite/tests";
    private const string _remoteSchemasPath = @"../../../../../ref-repos/JSON-Schema-Test-Suite/remotes";
    
    private const bool _useExternal = false;
    private const bool _runDraftNext = true;
    private const string _externalTestCasesPath = @"../../../../../../JSON-Schema-Test-Suite/tests";
    private const string _externalRemoteSchemasPath = @"../../../../../../JSON-Schema-Test-Suite/remotes";
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true
    };

    public static IEnumerable<TestCaseData> GetTestScenarios()
    {
        var testCasesPath = _useExternal ? _externalTestCasesPath : _testCasesPath;
        
        string[] draftVersions = ["draft6", "draft7", "draft2019-09", "draft2020-12"];
        if (_runDraftNext)
            draftVersions = draftVersions.Concat(["draft-next"]).ToArray();

        foreach (var draftVersion in draftVersions)
        {
            var draftPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, testCasesPath, draftVersion)
                .AdjustForPlatform();
            if (!Directory.Exists(draftPath))
                continue;

            var testFiles = Directory.GetFiles(draftPath, "*.json", SearchOption.AllDirectories);
            foreach (var testFile in testFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(testFile);
                var json = File.ReadAllText(testFile);
                
                SchemaTest[] tests;
                try
                {
                    tests = JsonSerializer.Deserialize<SchemaTest[]>(json, JsonOptions) 
                        ?? throw new InvalidOperationException($"Deserialization returned null for {testFile}");
                }
                catch (JsonException ex)
                {
                    TestConsole.WriteLine($"Error deserializing {testFile}:");
                    TestConsole.WriteLine($"JSON content: {json}");
                    TestConsole.WriteLine($"Error: {ex.Message}");
                    throw;
                }

                foreach (var test in tests)
                {
                    foreach (var validationCase in test.Tests)
                    {
                        var isOptional = testFile.Contains("optional");
                        var scenario = new TestScenario
                        {
                            DraftVersion = draftVersion,
                            FileName = fileName,
                            TestDescription = test.Description,
                            CaseDescription = validationCase.Description,
                            Schema = test.Schema,
                            Data = validationCase.Data,
                            ExpectedValid = validationCase.Valid,
                            IsOptional = isOptional
                        };

                        var optional = isOptional ? "(optional) / " : null;
                        var name = $"{draftVersion} / {fileName} / {optional}{test.Description} / {validationCase.Description}";
                        yield return new TestCaseData(scenario) { TestName = name };
                    }
                }
            }
        }
    }

    [OneTimeSetUp]
    public void LoadRemoteSchemas()
    {
	    var remotesPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, _useExternal
			    ? _externalRemoteSchemasPath
			    : _remoteSchemasPath)
		    .AdjustForPlatform();
        if (!Directory.Exists(remotesPath)) 
            throw new Exception("Cannot find remotes folder");

        var fileNames = Directory.GetFiles(remotesPath, "*.json", SearchOption.AllDirectories);

        foreach (var fileName in fileNames)
        {
            var schemaJson = File.ReadAllText(fileName);
            var schemaElement = JsonDocument.Parse(schemaJson).RootElement;
            var uri = new Uri(fileName.Replace(remotesPath, "http://localhost:1234").Replace('\\', '/'));
            // Note: Will need to register schemas once SchemaRegistry is available in new implementation
             SchemaRegistry.Global.Register(schemaElement, uri);
        }
    }

    [TestCaseSource(nameof(GetTestScenarios))]
    public void Run(TestScenario scenario)
    {
        TestConsole.WriteLine($"Running test: {scenario.TestDescription} - {scenario.CaseDescription}");
        TestConsole.WriteLine($"Draft: {scenario.DraftVersion}, File: {scenario.FileName}");
        TestConsole.WriteLine("Schema:");
        TestConsole.WriteLine(JsonSerializer.Serialize(scenario.Schema, JsonOptions));
        TestConsole.WriteLine("Instance:");
        TestConsole.WriteLine(JsonSerializer.Serialize(scenario.Data, JsonOptions));
        TestConsole.WriteLine($"Expected: {scenario.ExpectedValid}");
        TestConsole.WriteLine(string.Empty);

        if (!InstanceIsDeserializable(scenario.Data))
        {
            Assert.Inconclusive("Instance not deserializable");
            return;
        }

        try
        {
            var schemaNode = JsonSchema.Build(scenario.Schema);
            var result = Evaluator.Evaluate(schemaNode, scenario.Data);
            var isValid = result.IsValid;

            TestConsole.WriteLine("Evaluation Result:");
            TestConsole.WriteLine(JsonSerializer.Serialize(result, JsonOptions));
            TestConsole.WriteLine();

            if (scenario.IsOptional && isValid != scenario.ExpectedValid)
            {
                Assert.Inconclusive("Test optional");
                return;
            }

            if (isValid != scenario.ExpectedValid)
                Assert.Fail($"Test case failed. Expected: {scenario.ExpectedValid}, Actual: {isValid}");
        }
        catch (Exception ex)
        {
            TestConsole.WriteLine($"Error running test: {ex.Message}");
            TestConsole.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private static bool InstanceIsDeserializable(JsonElement testData)
    {
        try
        {
            // Handle different JsonElement types
            switch (testData.ValueKind)
            {
                case JsonValueKind.String:
                    return true;
                case JsonValueKind.Number:
                    return testData.TryGetDecimal(out _);
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:
                case JsonValueKind.Array:
                case JsonValueKind.Object:
                    return true;
                default:
                    return false;
            }
        }
        catch (Exception e)
        {
            TestConsole.WriteLine(e.Message);
            return false;
        }
    }

    [Test]
    public void EnsureTestSuiteConfiguredForServerBuild()
    {
        Assert.That(_useExternal, Is.False);
        //Assert.That(_runDraftNext, Is.False);
    }
} 