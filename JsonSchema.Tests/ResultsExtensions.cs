using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using NUnit.Framework;

namespace Json.Schema.Tests;

public static class ResultsExtensions
{
	public static void AssertInvalid(this ValidationResults results, string? expected = null)
	{
		Console.WriteLine(JsonSerializer.Serialize(results, new JsonSerializerOptions
		{
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		}));

		Assert.False(results.IsValid);
		AssertEquivalent(results, expected);
	}

	public static void AssertValid(this ValidationResults results, string? expected = null)
	{
		Console.WriteLine(JsonSerializer.Serialize(results, new JsonSerializerOptions
		{
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		}));

		Assert.True(results.IsValid);
		AssertEquivalent(results, expected);
	}

	private static void AssertEquivalent(ValidationResults results, string? expected = null)
	{
		if (expected == null) return;

		var expectedJson = JsonNode.Parse(expected);
		var actualJson = JsonNode.Parse(JsonSerializer.Serialize(results, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));

		Assert.IsTrue(expectedJson.IsEquivalentTo(actualJson));
	}
}