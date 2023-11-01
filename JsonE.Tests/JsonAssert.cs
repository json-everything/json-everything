using System.Text.Json.Nodes;
using Json.More;
using NUnit.Framework;

namespace Json.JsonE.Tests;

public static class JsonAssert
{
	public static void AreEquivalent(JsonNode? expected, JsonNode? actual)
	{
		if (!expected.IsEquivalentTo(actual))
			Assert.Fail($"Expected: {expected.AsJsonString()}\nActual: {actual.AsJsonString()}");
	}

	public static void IsNull(JsonNode? actual)
	{
		if (actual != null)
			Assert.Fail($"Expected: null\nActual: {actual.AsJsonString()}");
	}

	public static void IsTrue(JsonNode? actual)
	{
		if (actual is not JsonValue value || !value.TryGetValue(out bool b) || !b)
			Assert.Fail($"Expected: true\nActual: {actual.AsJsonString()}");
	}

	public static void IsFalse(JsonNode? actual)
	{
		if (actual is not JsonValue value || !value.TryGetValue(out bool b) || b)
			Assert.Fail($"Expected: true\nActual: {actual.AsJsonString()}");
	}
}