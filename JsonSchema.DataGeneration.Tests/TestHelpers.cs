using System;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.DataGeneration.Tests;

public static class TestHelpers
{
	public static void Run(JsonSchema schema, EvaluationOptions? options = null)
	{
		var result = schema.GenerateData();

		options ??= EvaluationOptions.Default;

		Assert.IsTrue(result.IsSuccess, "failed generation");
		Console.WriteLine(JsonSerializer.Serialize(result.Result,
			new JsonSerializerOptions
			{
				WriteIndented = true,
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			}));
		var validation = schema.Evaluate(result.Result, options);
		Console.WriteLine(JsonSerializer.Serialize(validation,
			new JsonSerializerOptions
			{
				WriteIndented = true,
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			}));
		Assert.IsTrue(validation.IsValid, "failed validation");
	}

	public static void RunFailure(JsonSchema schema, EvaluationOptions? options = null)
	{
		var result = schema.GenerateData();

		options ??= EvaluationOptions.Default;

		Console.WriteLine(result.ErrorMessage);
		if (result.IsSuccess)
			Console.WriteLine(JsonSerializer.Serialize(result.Result,
				new JsonSerializerOptions
				{
					WriteIndented = true,
					Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
				}));
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