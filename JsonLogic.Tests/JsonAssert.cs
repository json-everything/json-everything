using System.Text.Json;
using Json.More;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public static class JsonAssert
	{
		public static void AreEquivalent(JsonElement expected, JsonElement actual)
		{
			if (!expected.IsEquivalentTo(actual)) 
				Assert.Fail($"Expected: {expected.ToJsonString()}\nActual: {actual.ToJsonString()}");
		}

		public static void AreEquivalent(int expected, JsonElement actual)
		{
			var expectedJson = expected.AsJsonElement();
			if (!expectedJson.IsEquivalentTo(actual)) 
				Assert.Fail($"Expected: {expectedJson.ToJsonString()}\nActual: {actual.ToJsonString()}");
		}

		public static void AreEquivalent(long expected, JsonElement actual)
		{
			var expectedJson = expected.AsJsonElement();
			if (!expectedJson.IsEquivalentTo(actual)) 
				Assert.Fail($"Expected: {expectedJson.ToJsonString()}\nActual: {actual.ToJsonString()}");
		}

		public static void AreEquivalent(float expected, JsonElement actual)
		{
			var expectedJson = expected.AsJsonElement();
			if (!expectedJson.IsEquivalentTo(actual)) 
				Assert.Fail($"Expected: {expectedJson.ToJsonString()}\nActual: {actual.ToJsonString()}");
		}

		public static void AreEquivalent(double expected, JsonElement actual)
		{
			var expectedJson = expected.AsJsonElement();
			if (!expectedJson.IsEquivalentTo(actual)) 
				Assert.Fail($"Expected: {expectedJson.ToJsonString()}\nActual: {actual.ToJsonString()}");
		}

		public static void AreEquivalent(decimal expected, JsonElement actual)
		{
			var expectedJson = expected.AsJsonElement();
			if (!expectedJson.IsEquivalentTo(actual)) 
				Assert.Fail($"Expected: {expectedJson.ToJsonString()}\nActual: {actual.ToJsonString()}");
		}

		public static void AreEquivalent(string expected, JsonElement actual)
		{
			var expectedJson = expected.AsJsonElement();
			if (!expectedJson.IsEquivalentTo(actual)) 
				Assert.Fail($"Expected: {expectedJson.ToJsonString()}\nActual: {actual.ToJsonString()}");
		}

		public static void AreEquivalent(bool expected, JsonElement actual)
		{
			var expectedJson = expected.AsJsonElement();
			if (!expectedJson.IsEquivalentTo(actual)) 
				Assert.Fail($"Expected: {expectedJson.ToJsonString()}\nActual: {actual.ToJsonString()}");
		}

		public static void IsNull(JsonElement actual)
		{
			if (actual.ValueKind != JsonValueKind.Null)
				Assert.Fail($"Expected: null\nActual: {actual.ToJsonString()}");
		}

		public static void IsTrue(JsonElement actual)
		{
			if (actual.ValueKind != JsonValueKind.True)
				Assert.Fail($"Expected: true\nActual: {actual.ToJsonString()}");
		}

		public static void IsFalse(JsonElement actual)
		{
			if (actual.ValueKind != JsonValueKind.False) 
				Assert.Fail($"Expected: false\nActual: {actual.ToJsonString()}");
		}
	}
}