using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests
{
	public static class ResultsExtensions
	{
		public static void AssertInvalid(this ValidationResults results)
		{
			Console.WriteLine(JsonSerializer.Serialize(results,  new JsonSerializerOptions
			{
				WriteIndented = true, 
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			}));

			Assert.False(results.IsValid);
		}
		public static void AssertValid(this ValidationResults results)
		{
			Console.WriteLine(JsonSerializer.Serialize(results,  new JsonSerializerOptions
			{
				WriteIndented = true, 
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			}));

			Assert.True(results.IsValid);
		}
	}
}