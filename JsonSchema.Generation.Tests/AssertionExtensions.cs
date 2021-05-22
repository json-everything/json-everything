using System;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Generation.Tests
{
	internal static class AssertionExtensions
	{
		public static void AssertEqual(JsonSchema expected, JsonSchema actual)
		{
			Console.WriteLine(JsonSerializer.Serialize(expected, new JsonSerializerOptions { WriteIndented = true }));
			Console.WriteLine();
			Console.WriteLine(JsonSerializer.Serialize(actual, new JsonSerializerOptions { WriteIndented = true }));
			Assert.AreEqual(expected, actual);
		}
	}
}
