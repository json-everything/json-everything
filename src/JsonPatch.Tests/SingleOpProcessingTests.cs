using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using NUnit.Framework;
using TestHelpers;

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
			TestEnvironment.SerializerOptions)!;

		Assert.Multiple(() =>
		{
			Assert.That(patch.Operations, Has.Count.EqualTo(3));
			Assert.That(patch.Operations[0].Op, Is.EqualTo(OperationType.Replace));
			Assert.That(patch.Operations[1].Op, Is.EqualTo(OperationType.Add));
			Assert.That(patch.Operations[2].Op, Is.EqualTo(OperationType.Remove));
		});
	}

	[Test]
	public void Add_Local()
	{
		var patch = JsonSerializer.Deserialize<JsonPatch>(
			"[{ \"op\": \"add\", \"path\": \"/hello\", \"value\": [\"world\"] }]",
			TestEnvironment.SerializerOptions)!;

		var element = JsonNode.Parse("{\"something\":\"added\"}");
		var expected = JsonNode.Parse("{\"something\":\"added\",\"hello\":[\"world\"]}");

		var actual = patch.Apply(element);

		Assert.Multiple(() =>
		{
			Assert.That(actual.Error, Is.Null);
			Assert.That(expected.IsEquivalentTo(actual.Result), Is.True);
		});
	}

	[Test]
	public void Add_Nested()
	{
		var patch = JsonSerializer.Deserialize<JsonPatch>(
			"[{ \"op\": \"add\", \"path\": \"/inserted/hello\", \"value\": [\"world\"] }]",
			TestEnvironment.SerializerOptions)!;

		var element = JsonNode.Parse("{\"something\":\"added\",\"inserted\":{}}");
		var expected = JsonNode.Parse("{\"something\":\"added\",\"inserted\":{\"hello\":[\"world\"]}}");

		var actual = patch.Apply(element);

		TestConsole.WriteLine(actual.Result.AsJsonString());

		Assert.Multiple(() =>
		{
			Assert.That(actual.Error, Is.Null);
			Assert.That(expected.IsEquivalentTo(actual.Result), Is.True);
		});
	}

	[Test]
	public void Add_Nested_ReplacesValue()
	{
		var patch = JsonSerializer.Deserialize<JsonPatch>(
			"[{ \"op\": \"add\", \"path\": \"/inserted/hello\", \"value\": [\"world\"] }]",
			TestEnvironment.SerializerOptions)!;

		var element = JsonNode.Parse("{\"something\":\"added\",\"inserted\":{\"hello\":\"replace me\"}}");
		var expected = JsonNode.Parse("{\"something\":\"added\",\"inserted\":{\"hello\":[\"world\"]}}");

		var actual = patch.Apply(element);

		TestConsole.WriteLine(actual.Result.AsJsonString());

		Assert.Multiple(() =>
		{
			Assert.That(actual.Error, Is.Null);
			Assert.That(expected.IsEquivalentTo(actual.Result), Is.True);
		});
	}

	[Test]
	public void Add_NestedPathNotFound()
	{
		var patch = JsonSerializer.Deserialize<JsonPatch>(
			"[{ \"op\": \"add\", \"path\": \"/inserted/hello\", \"value\": [\"world\"] }]",
			TestEnvironment.SerializerOptions)!;

		var element = JsonNode.Parse("{\"something\":\"added\",\"insert here\":{}}");

		var actual = patch.Apply(element);

		TestConsole.WriteLine(actual.Result.AsJsonString());

		Assert.That(actual.Error, Is.EqualTo("Target path `/inserted/hello` could not be reached."));
	}

	[Test]
	public void Replace_Local()
	{
		var patch = JsonSerializer.Deserialize<JsonPatch>(
			"[{ \"op\": \"replace\", \"path\": \"/something\", \"value\": \"boo\" }]",
			TestEnvironment.SerializerOptions)!;

		var element = JsonNode.Parse("{\"something\":\"added\"}");
		var expected = JsonNode.Parse("{\"something\":\"boo\"}");

		var actual = patch.Apply(element);

		Assert.Multiple(() =>
		{
			Assert.That(actual.Error, Is.Null);
			Assert.That(expected.IsEquivalentTo(actual.Result), Is.True);
		});
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
			TestEnvironment.SerializerOptions)!;

		var element = new PatchExtensionTests.TestModel{ Numbers = [1, 2, 3, 4] };

		var actual = patch.Apply(element, TestEnvironment.SerializerOptions);

		Assert.That(actual?.Numbers, Is.EqualTo(expected).AsCollection);
	}
}