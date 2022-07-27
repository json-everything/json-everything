using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using Json.Pointer;
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
		var actualJson = JsonSerializer.SerializeToNode(results);

		var (areEquivalent, errorLocation) = AreEquivalent(expectedJson, actualJson);
		if (!areEquivalent) 
			Assert.Fail($"Equivalence test failed at {errorLocation}");
	}

	private static (bool, JsonPointer?) AreEquivalent(JsonNode? a, JsonNode? b, JsonPointer? currentLocation = null)
	{
		currentLocation ??= JsonPointer.Empty;
		switch (a, b)
		{
			case (null, null):
				return (true, currentLocation);
			case (JsonObject objA, JsonObject objB):
				if (objA.Count != objB.Count) return (false, currentLocation);
				var grouped = objA.Concat(objB)
					.GroupBy(p => p.Key)
					.Select(g => new{g.Key, Values = g.Select(x => x.Value).ToList()})
					.ToList();
				foreach (var group in grouped)
				{
					if (group.Values.Count != 2) return (false, currentLocation.Combine(group.Key));
					var (areEquivalent, errorLocation) = AreEquivalent(group.Values[0], group.Values[1], currentLocation.Combine(group.Key));
					if (!areEquivalent) return (false, errorLocation);
				}
				return (true, currentLocation);
			case (JsonArray arrayA, JsonArray arrayB):
				if (arrayA.Count != arrayB.Count) return (false, currentLocation);
				var zipped = arrayA.Zip(arrayB, (ae, be) => (ae, be));
				int index = 0;
				foreach (var item in zipped)
				{
					var (areEquivalent, errorLocation) = AreEquivalent(item.ae, item.be, currentLocation.Combine(index++));
					if (!areEquivalent) return (false, errorLocation);
				}
				return (true, currentLocation);
			case (JsonValue aValue, JsonValue bValue):
				if (aValue.GetValue<object>() is JsonElement aElement &&
				    bValue.GetValue<object>() is JsonElement bElement)
					return (aElement.IsEquivalentTo(bElement), currentLocation);
				return (a.ToJsonString() == b.ToJsonString(), currentLocation);
			default:
				return (a?.ToJsonString() == b?.ToJsonString(), currentLocation);
		}
	}
}