using System;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.DataGeneration.Tests;

public static class TestRunner
{
	public static readonly JsonSerializerOptions SerializerOptions =
		new()
		{
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
			TypeInfoResolverChain = { DataGenerationTestsSerializerContext.Default }
		};

	public static void Run(JsonSchema schema)
	{
		var result = schema.GenerateData();
		if (!result.IsSuccess)
		{
			if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
				TestConsole.WriteLine(result.ErrorMessage);

			TestConsole.WriteLine(JsonSerializer.Serialize(result, SerializerOptions));
		}

		Assert.That(result.IsSuccess, Is.True, "failed generation");
		var resultElt = JsonSerializer.SerializeToElement(result.Result, SerializerOptions);
		TestConsole.WriteLine(resultElt.GetRawText());
		var validation = schema.Evaluate(resultElt);
		TestConsole.WriteLine(JsonSerializer.Serialize(validation, SerializerOptions));
		if (!validation.IsValid)
		{
			TestConsole.WriteLine("Validation failed");
			if (validation.Details != null)
				TestConsole.WriteLine(JsonSerializer.Serialize(validation.Details, SerializerOptions));
		}
		Assert.That(validation.IsValid, Is.True, "failed validation");
	}

	public static void RunFailure(JsonSchema schema)
	{
		var result = schema.GenerateData();
		if (!result.IsSuccess)
		{
			if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
				TestConsole.WriteLine(result.ErrorMessage);

			TestConsole.WriteLine(JsonSerializer.Serialize(result, SerializerOptions));
		}
		if (result.IsSuccess)
			TestConsole.WriteLine(JsonSerializer.Serialize(result.Result, SerializerOptions));
		Assert.That(result.IsSuccess, Is.False, "generation succeeded");
	}

	public static void RunInLoopForDebugging(JsonSchema schema)
	{
		if (!Debugger.IsAttached)
			throw new InvalidOperationException("Don't call this unless you're debugging");

		while (true)
		{
			schema.GenerateData();
		}
	}
}

[JsonSerializable(typeof(GenerationResult))]
[JsonSerializable(typeof(JsonSchema))]
[JsonSerializable(typeof(EvaluationResults))]
internal partial class DataGenerationTestsSerializerContext : JsonSerializerContext;
