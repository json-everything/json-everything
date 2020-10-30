using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using Json.More;
using NUnit.Framework;

namespace Json.Schema.Tests
{
	public static class ResultsExtensions
	{
		public static void AssertInvalid(this ValidationResults results, string expected = null)
		{
			Console.WriteLine(JsonSerializer.Serialize(results,  new JsonSerializerOptions
			{
				WriteIndented = true, 
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			}));

			Assert.False(results.IsValid);
			AssertEquivalent(results, expected);
		}

		public static void AssertValid(this ValidationResults results, string expected = null)
		{
			Console.WriteLine(JsonSerializer.Serialize(results,  new JsonSerializerOptions
			{
				WriteIndented = true, 
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			}));

			Assert.True(results.IsValid);
			AssertEquivalent(results, expected);
		}

		private static void AssertEquivalent(ValidationResults results, string expected = null)
		{
			if (expected == null) return;

			var expectedJson = JsonDocument.Parse(expected).RootElement;
			var actualJson = JsonDocument.Parse(JsonSerializer.Serialize(results)).RootElement;

			Assert.IsTrue(expectedJson.IsEquivalentTo(actualJson));
		}
	}
}