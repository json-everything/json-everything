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
]")!;
		Assert.AreEqual(3, patch.Operations.Count);
		Assert.AreEqual(OperationType.Replace, patch.Operations[0].Op);
		Assert.AreEqual(OperationType.Add, patch.Operations[1].Op);
		Assert.AreEqual(OperationType.Remove, patch.Operations[2].Op);
	}

	[Test]
	public void Add_Local()
	{
		var patch = JsonSerializer.Deserialize<JsonPatch>(
			"[{ \"op\": \"add\", \"path\": \"/hello\", \"value\": [\"world\"] }]")!;

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
			"[{ \"op\": \"add\", \"path\": \"/inserted/hello\", \"value\": [\"world\"] }]")!;

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
			"[{ \"op\": \"add\", \"path\": \"/inserted/hello\", \"value\": [\"world\"] }]")!;

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
			"[{ \"op\": \"add\", \"path\": \"/inserted/hello\", \"value\": [\"world\"] }]")!;

		var element = JsonNode.Parse("{\"something\":\"added\",\"insert here\":{}}");

		var actual = patch.Apply(element);

		Console.WriteLine(actual.Result.AsJsonString());

		Assert.AreEqual("Target path `/inserted/hello` could not be reached.", actual.Error);
	}

	[Test]
	public void Replace_Local()
	{
		var patch = JsonSerializer.Deserialize<JsonPatch>(
			"[{ \"op\": \"replace\", \"path\": \"/something\", \"value\": \"boo\" }]")!;

		var element = JsonNode.Parse("{\"something\":\"added\"}");
		var expected = JsonNode.Parse("{\"something\":\"boo\"}");

		var actual = patch.Apply(element);

		Assert.IsNull(actual.Error);
		Assert.IsTrue(expected.IsEquivalentTo(actual.Result));
	}
}