using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.DataGeneration.Tests
{
	public static class TestHelpers
	{
		public static void Run(JsonSchema schema)
		{
			var result = schema.GenerateData();

			Assert.IsTrue(result.IsSuccess, "failed generation");
			Console.WriteLine(JsonSerializer.Serialize(result.Result,
				new JsonSerializerOptions
				{
					WriteIndented = true,
					Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
				}));
			Assert.IsTrue(schema.Validate(result.Result).IsValid, "failed validation");
		}
	}
}