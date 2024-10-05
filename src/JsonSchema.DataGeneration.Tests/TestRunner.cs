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

	public static void Run(JsonSchema schema, EvaluationOptions? options = null)
	{
		options ??= EvaluationOptions.Default;

		var result = schema.GenerateData(options);

		Assert.That(result.IsSuccess, Is.True, "failed generation");
		TestConsole.WriteLine(JsonSerializer.Serialize(result.Result, SerializerOptions));
		var validation = schema.Evaluate(result.Result, options);
		TestConsole.WriteLine(JsonSerializer.Serialize(validation, SerializerOptions));
		Assert.That(validation.IsValid, Is.True, "failed validation");
	}

	public static void RunFailure(JsonSchema schema, EvaluationOptions? options = null)
	{
		var result = schema.GenerateData(options);

		TestConsole.WriteLine(result.ErrorMessage);
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
