using System;
using System.Text.Json;
using Json.More;
using NUnit.Framework;

namespace Json.Patch.Tests
{
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

			var element = JsonDocument.Parse("{\"something\":\"added\"}").RootElement;
			var expected = JsonDocument.Parse("{\"something\":\"added\",\"hello\":[\"world\"]}").RootElement;

			var actual = patch.Apply(element);

			Assert.IsNull(actual.Error);
			Assert.IsTrue(expected.IsEquivalentTo(actual.Result));
		}

		[Test]
		public void Add_Nested()
		{
			var patch = JsonSerializer.Deserialize<JsonPatch>(
				"[{ \"op\": \"add\", \"path\": \"/inserted/hello\", \"value\": [\"world\"] }]")!;

			var element = JsonDocument.Parse("{\"something\":\"added\",\"inserted\":{}}").RootElement;
			var expected = JsonDocument.Parse("{\"something\":\"added\",\"inserted\":{\"hello\":[\"world\"]}}").RootElement;

			var actual = patch.Apply(element);

			Console.WriteLine(actual.Result.ToJsonString());

			Assert.IsNull(actual.Error);
			Assert.IsTrue(expected.IsEquivalentTo(actual.Result));
		}

		[Test]
		public void Add_Nested_ReplacesValue()
		{
			var patch = JsonSerializer.Deserialize<JsonPatch>(
				"[{ \"op\": \"add\", \"path\": \"/inserted/hello\", \"value\": [\"world\"] }]")!;

			var element = JsonDocument.Parse("{\"something\":\"added\",\"inserted\":{\"hello\":\"replace me\"}}").RootElement;
			var expected = JsonDocument.Parse("{\"something\":\"added\",\"inserted\":{\"hello\":[\"world\"]}}").RootElement;

			var actual = patch.Apply(element);

			Console.WriteLine(actual.Result.ToJsonString());

			Assert.IsNull(actual.Error);
			Assert.IsTrue(expected.IsEquivalentTo(actual.Result));
		}

		[Test]
		public void Add_NestedPathNotFound()
		{
			var patch = JsonSerializer.Deserialize<JsonPatch>(
				"[{ \"op\": \"add\", \"path\": \"/inserted/hello\", \"value\": [\"world\"] }]")!;

			var element = JsonDocument.Parse("{\"something\":\"added\",\"insert here\":{}}").RootElement;

			var actual = patch.Apply(element);

			Console.WriteLine(actual.Result.ToJsonString());

			Assert.AreEqual("Path `/inserted/hello` could not be reached.", actual.Error);
		}

		[Test]
		public void Replace_Local()
		{
			var patch = JsonSerializer.Deserialize<JsonPatch>(
				"[{ \"op\": \"replace\", \"path\": \"/something\", \"value\": \"boo\" }]")!;

			var element = JsonDocument.Parse("{\"something\":\"added\"}").RootElement;
			var expected = JsonDocument.Parse("{\"something\":\"boo\"}").RootElement;

			var actual = patch.Apply(element);

			Assert.IsNull(actual.Error);
			Assert.IsTrue(expected.IsEquivalentTo(actual.Result));
		}
	}
}