using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using NUnit.Framework;

namespace Json.JsonE.Tests;

public static class JsonAssert
{
	private static readonly JsonSerializerOptions _options = new()
	{
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};

	public static void AreEquivalent(JsonNode? expected, JsonNode? actual)
	{
		if (!expected.IsEquivalentTo(actual))
			Assert.Fail($"Expected: {expected.AsJsonString(_options)}\nActual: {actual.AsJsonString(_options)}");
	}

	public static void IsNull(JsonNode? actual)
	{
		if (actual != null)
			Assert.Fail($"Expected: null\nActual: {actual.AsJsonString(_options)}");
	}

	public static void IsTrue(JsonNode? actual)
	{
		if (actual is not JsonValue value || !value.TryGetValue(out bool b) || !b)
			Assert.Fail($"Expected: true\nActual: {actual.AsJsonString(_options)}");
	}

	public static void IsFalse(JsonNode? actual)
	{
		if (actual is not JsonValue value || !value.TryGetValue(out bool b) || b)
			Assert.Fail($"Expected: true\nActual: {actual.AsJsonString(_options)}");
	}
}