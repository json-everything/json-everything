using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.DataGeneration.Tests
{
	public static class TestHelpers
	{
		public static void Run(JsonSchema schema, ValidationOptions? options = null)
		{
			var result = schema.GenerateData();

			options ??= ValidationOptions.Default;

			Assert.IsTrue(result.IsSuccess, "failed generation");
			Console.WriteLine(JsonSerializer.Serialize(result.Result,
				new JsonSerializerOptions
				{
					WriteIndented = true,
					Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
				}));
			Assert.IsTrue(schema.Validate(result.Result, options).IsValid, "failed validation");
		}

		public static void RunInLoopForDebugging(JsonSchema schema)
		{
			while (true)
			{
				schema.GenerateData();
			}
		}
	}
}