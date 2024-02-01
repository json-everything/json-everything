using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using NUnit.Framework;

namespace Json.Patch.Tests;

public class SingleOpProcessingTests
{
	[Test]
	public void Test1()
	{
		var patch = JsonSerializer.Deserialize<JsonPatch>(@"[
  { ""op"": ""replace"", ""path"": ""/baz"", ""value"": ""boo"" },
  { ""op"": ""add"", ""path"": ""/hello"", ""value"": [""world""] },
  { ""op"": ""remove"", ""path"": ""/foo"" }
]",
			PatchSerializerContext.Default.JsonPatch)!;

		Assert.AreEqual(3, patch.Operations.Count);
		Assert.AreEqual(OperationType.Replace, patch.Operations[0].Op);
		Assert.AreEqual(OperationType.Add, patch.Operations[1].Op);
		Assert.AreEqual(OperationType.Remove, patch.Operations[2].Op);
	}

	[Test]
	public void Add_Local()
	{
		var patch = JsonSerializer.Deserialize<JsonPatch>(
			"[{ \"op\": \"add\", \"path\": \"/hello\", \"value\": [\"world\"] }]",
			PatchSerializerContext.Default.JsonPatch)!;

		var element = JsonNode.Parse("{\"something\":\"added\"}");
		var expected = JsonNode.Parse("{\"something\":\"added\",\"hello\":[\"world\"]}");

		var actual = patch.Apply(element);

		Assert.IsNull(actual.Error);
		Assert.IsTrue(expected.IsEquivalentTo(actual.Result));
	}

	[Test]
	public void Add_Nested()
	{
		var patch = JsonSerializer.Deserialize<JsonPatch>(
			"[{ \"op\": \"add\", \"path\": \"/inserted/hello\", \"value\": [\"world\"] }]",
			PatchSerializerContext.Default.JsonPatch)!;

		var element = JsonNode.Parse("{\"something\":\"added\",\"inserted\":{}}");
		var expected = JsonNode.Parse("{\"something\":\"added\",\"inserted\":{\"hello\":[\"world\"]}}");

		var actual = patch.Apply(element);

		Console.WriteLine(actual.Result.AsJsonString());

		Assert.IsNull(actual.Error);
		Assert.IsTrue(expected.IsEquivalentTo(actual.Result));
	}

	[Test]
	public void Add_Nested_ReplacesValue()
	{
		var patch = JsonSerializer.Deserialize<JsonPatch>(
			"[{ \"op\": \"add\", \"path\": \"/inserted/hello\", \"value\": [\"world\"] }]",
			PatchSerializerContext.Default.JsonPatch)!;

		var element = JsonNode.Parse("{\"something\":\"added\",\"inserted\":{\"hello\":\"replace me\"}}");
		var expected = JsonNode.Parse("{\"something\":\"added\",\"inserted\":{\"hello\":[\"world\"]}}");

		var actual = patch.Apply(element);

		Console.WriteLine(actual.Result.AsJsonString());

		Assert.IsNull(actual.Error);
		Assert.IsTrue(expected.IsEquivalentTo(actual.Result));
	}

	[Test]
	public void Add_NestedPathNotFound()
	{
		var patch = JsonSerializer.Deserialize<JsonPatch>(
			"[{ \"op\": \"add\", \"path\": \"/inserted/hello\", \"value\": [\"world\"] }]",
			PatchSerializerContext.Default.JsonPatch)!;

		var element = JsonNode.Parse("{\"something\":\"added\",\"insert here\":{}}");

		var actual = patch.Apply(element);

		Console.WriteLine(actual.Result.AsJsonString());

		Assert.AreEqual("Target path `/inserted/hello` could not be reached.", actual.Error);
	}

	[Test]
	public void Replace_Local()
	{
		var patch = JsonSerializer.Deserialize<JsonPatch>(
			"[{ \"op\": \"replace\", \"path\": \"/something\", \"value\": \"boo\" }]",
			PatchSerializerContext.Default.JsonPatch)!;

		var element = JsonNode.Parse("{\"something\":\"added\"}");
		var expected = JsonNode.Parse("{\"something\":\"boo\"}");

		var actual = patch.Apply(element);

		Assert.IsNull(actual.Error);
		Assert.IsTrue(expected.IsEquivalentTo(actual.Result));
	}

	[TestCase(0, 1, new[] { 2, 1, 3, 4 })]
	[TestCase(1, 0, new[] { 2, 1, 3, 4 })]
	[TestCase(1, 2, new[] { 1, 3, 2, 4 })]
	[TestCase(0, 2, new[] { 3, 1, 2, 4 })]
	[TestCase(1, 3, new[] { 1, 4, 2, 3 })]
	[TestCase(2, 3, new[] { 1, 2, 4, 3 })]
	[TestCase(3, 1, new[] { 1, 3, 4, 2 })]
	public void Move_Array(int to, int from, int[] expected)
	{
		var patchStr = "[{ \"op\": \"move\", \"path\": \"/Numbers/" + to + "\", \"from\": \"/Numbers/" + from + "\" }]";
		var patch = JsonSerializer.Deserialize<JsonPatch>(patchStr,
			PatchSerializerContext.Default.JsonPatch)!;

		var element = new PatchExtensionTests.TestModel{ Numbers = [1, 2, 3, 4] };

		var actual = patch.Apply(element, TestSerializerContext.Default.Options);

		CollectionAssert.AreEqual(expected, actual?.Numbers);
	}
}