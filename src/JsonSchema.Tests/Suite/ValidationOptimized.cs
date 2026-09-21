using System;
using System.Collections.Generic;
using System.Text.Json;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.Tests.Suite;

public class ValidationOptimized
{
	private const bool _useExternal = false;
	private const bool _runV1 = false;

	public static IEnumerable<TestCaseData> TestCases() =>
		ValidationTestLoader.GetTests(_useExternal, _runV1, OutputFormat.Flag);

	[TestCaseSource(nameof(TestCases))]
	public void Test(TestCollection collection, TestCase test, string fileName, BuildOptions buildOptions, EvaluationOptions evaluationOptions)
	{
		TestConsole.WriteLine();
		TestConsole.WriteLine();
		TestConsole.WriteLine(fileName);
		TestConsole.WriteLine(collection.Description);
		TestConsole.WriteLine(test.Description);
		TestConsole.WriteLine(test.Valid ? "valid" : "invalid");
		TestConsole.WriteLine();
		TestConsole.WriteLine(JsonSerializer.Serialize(collection.Schema, TestEnvironment.TestOutputSerializerOptions));
		TestConsole.WriteLine();
		TestConsole.WriteLine(JsonSerializer.Serialize(test.Data, TestEnvironment.TestOutputSerializerOptions));
		TestConsole.WriteLine();

		JsonSchema schema;
		try
		{
			schema = Measure.Run("Build", () => JsonSchema.Build(collection.Schema, buildOptions));
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			if (collection.IsOptional)
			{
				Assert.Inconclusive();
				return;
			}

			throw;
		}

		EvaluationResults result;
		try
		{
			result = Measure.Run("Evaluate", () => schema.Evaluate(test.Data, evaluationOptions));
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			if (collection.IsOptional)
			{
				Assert.Inconclusive();
				return;
			}

			throw;
		}

		//result.ToBasic();
		TestConsole.WriteLine();
		TestConsole.WriteLine("Result:");
		TestConsole.WriteLine(JsonSerializer.Serialize(result, TestEnvironment.TestOutputSerializerOptions));

		if (collection.IsOptional && result.IsValid != test.Valid)
			Assert.Inconclusive("Test optional");
		Assert.That(result.IsValid, Is.EqualTo(test.Valid));
	}

	[Test]
	public void EnsureTestSuiteConfiguredForServerBuild()
	{
		Assert.That(_useExternal, Is.False);
		//Assert.IsFalse(_runV1);
	}
}
