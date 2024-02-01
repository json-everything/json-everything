using System;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;

namespace Json.Schema.DataGeneration.Tests;

public static class TestHelpers
{
	public static readonly JsonSerializerOptions DataGenSerializerOptions =
		new(DataGenerationTestsSerializerContext.Default.Options)
	{
		WriteIndented = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};

	public static readonly JsonSerializerOptions SchemaSerializerOptions =
		new(JsonSchemaSerializerContext.Default.Options)
	{
		WriteIndented = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};

	public static void Run(JsonSchema schema, EvaluationOptions? options = null)
	{
		var result = schema.GenerateData();

		options ??= EvaluationOptions.Default;

		Assert.IsTrue(result.IsSuccess, "failed generation");
		Console.WriteLine(JsonSerializer.Serialize(result.Result, DataGenSerializerOptions));
		var validation = schema.Evaluate(result.Result, options);
		Console.WriteLine(JsonSerializer.Serialize(validation, SchemaSerializerOptions));
		Assert.IsTrue(validation.IsValid, "failed validation");
	}

	public static void RunFailure(JsonSchema schema)
	{
		var result = schema.GenerateData();

		Console.WriteLine(result.ErrorMessage);
		if (result.IsSuccess)
			Console.WriteLine(JsonSerializer.Serialize(result.Result, DataGenSerializerOptions));
		Assert.IsFalse(result.IsSuccess, "generation succeeded");
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
internal partial class DataGenerationTestsSerializerContext : JsonSerializerContext;