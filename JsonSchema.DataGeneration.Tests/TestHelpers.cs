using System;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;

namespace Json.Schema.DataGeneration.Tests;

public static class TestHelpers
{
	public static readonly JsonSerializerOptions SerializerOptions =
		new JsonSerializerOptions
	{
		WriteIndented = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		TypeInfoResolverChain = { DataGenerationTestsSerializerContext.Default }
	}.WithJsonSchema();

	public static void Run(JsonSchema schema, EvaluationOptions? options = null)
	{
		var result = schema.GenerateData();

		options ??= EvaluationOptions.Default;

		Assert.IsTrue(result.IsSuccess, "failed generation");
		Console.WriteLine(JsonSerializer.Serialize(result.Result, SerializerOptions));
		var validation = schema.Evaluate(result.Result, options);
		Console.WriteLine(JsonSerializer.Serialize(validation, SerializerOptions));
		Assert.IsTrue(validation.IsValid, "failed validation");
	}

	public static void RunFailure(JsonSchema schema)
	{
		var result = schema.GenerateData();

		Console.WriteLine(result.ErrorMessage);
		if (result.IsSuccess)
			Console.WriteLine(JsonSerializer.Serialize(result.Result, SerializerOptions));
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
