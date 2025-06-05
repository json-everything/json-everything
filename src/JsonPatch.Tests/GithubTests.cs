﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using Json.Pointer;
using NUnit.Framework;
using TestHelpers;

namespace Json.Patch.Tests;

public class GithubTests
{
	[Test]
	public void Issue393_PatchDoesNothing()
	{
		const string mask = "*****";
		var maskJson = JsonValue.Create(mask);

		var pathsToPatch = new[] { "/first_name", "/last_name" };

		var patchOperations = pathsToPatch.Select(path => PatchOperation.Replace(JsonPointer_Old.Parse(path), maskJson));
		var patchConfig = new JsonPatch(patchOperations);

		TestConsole.WriteLine(JsonSerializer.Serialize(patchConfig, TestSerializerContext.Default.JsonPatch));

		const string singleObjectJson = "{" +
										"\"_id\":\"640729d45434f90313d25c78\"," +
										"\"guid\":\"f2e2767c-03e0-4862-addc-7d46c55efb33\"," +
										"\"first_name\":\"Kathrine\"," +
										"\"last_name\":\"Pate\"" +
										"}";

		var singleObject = JsonNode.Parse(singleObjectJson);
		var patchedSingleObject = patchConfig.Apply(singleObject).Result;
		TestConsole.WriteLine(JsonSerializer.Serialize(patchedSingleObject, TestSerializerContext.Default.JsonNode!));

		const string arrayObjectJson = "[" +
									   "{" +
									   "\"_id\":\"640729d45434f90313d25c78\"," +
									   "\"guid\":\"f2e2767c-03e0-4862-addc-7d46c55efb33\"," +
									   "\"first_name\":\"Kathrine\"," +
									   "\"last_name\":\"Pate\"" +
									   "}," +
									   "{\"_id\":\"640729d45b5824ffcabc30a5\"," +
									   "\"guid\":\"73193eda-074b-4f31-9f09-507a008ccb75\"," +
									   "\"first_name\":\"Rivers\"," +
									   "\"last_name\":\"Smith\"" +
									   "}" +
									   "]";

		var arrayObject = JsonNode.Parse(arrayObjectJson);

		// Way 1: patch whole array
		var patchedArray = patchConfig.Apply(arrayObject).Result; // <- does nothing

		TestConsole.WriteLine(JsonSerializer.Serialize(patchedArray, TestSerializerContext.Default.JsonNode!));
	}

	[Test]
	public void Issue393_NodeAlreadyHasParent_2()
	{
		const string mask = "*****";
		var maskJson = JsonValue.Create(mask);

		var pathsToPatch = new[] { "/first_name", "/last_name" };

		var patchOperations = pathsToPatch.Select(path => PatchOperation.Replace(JsonPointer_Old.Parse(path), maskJson));
		var patchConfig = new JsonPatch(patchOperations);

		const string singleObjectJson = "{" +
										"\"_id\":\"640729d45434f90313d25c78\"," +
										"\"guid\":\"f2e2767c-03e0-4862-addc-7d46c55efb33\"," +
										"\"first_name\":\"Kathrine\"," +
										"\"last_name\":\"Pate\"" +
										"}";

		var singleObject = JsonNode.Parse(singleObjectJson);
		var patchedSingleObject = patchConfig.Apply(singleObject).Result;
		TestConsole.WriteLine(JsonSerializer.Serialize(patchedSingleObject, TestEnvironment.SerializerOptions));

		const string arrayObjectJson = "[" +
									   "{" +
									   "\"_id\":\"640729d45434f90313d25c78\"," +
									   "\"guid\":\"f2e2767c-03e0-4862-addc-7d46c55efb33\"," +
									   "\"first_name\":\"Kathrine\"," +
									   "\"last_name\":\"Pate\"" +
									   "}," +
									   "{\"_id\":\"640729d45b5824ffcabc30a5\"," +
									   "\"guid\":\"73193eda-074b-4f31-9f09-507a008ccb75\"," +
									   "\"first_name\":\"Rivers\"," +
									   "\"last_name\":\"Smith\"" +
									   "}" +
									   "]";

		var arrayObject = JsonNode.Parse(arrayObjectJson);

		var jsonArray = arrayObject!.AsArray();

		// Way 2: just patch every element
		foreach (var element in jsonArray)
		{
			var patchedNode = patchConfig.Apply(element).Result; // <-  throws an error
			TestConsole.WriteLine(JsonSerializer.Serialize(patchedNode, TestEnvironment.SerializerOptions));
		}
	}

	[Test]
	public void Issue393_NodeAlreadyHasParent_3()
	{
		const string mask = "*****";
		var maskJson = JsonValue.Create(mask);

		var pathsToPatch = new[] { "/first_name", "/last_name" };

		var patchOperations = pathsToPatch.Select(path => PatchOperation.Replace(JsonPointer_Old.Parse(path), maskJson));
		var patchConfig = new JsonPatch(patchOperations);

		const string singleObjectJson = "{" +
										"\"_id\":\"640729d45434f90313d25c78\"," +
										"\"guid\":\"f2e2767c-03e0-4862-addc-7d46c55efb33\"," +
										"\"first_name\":\"Kathrine\"," +
										"\"last_name\":\"Pate\"" +
										"}";

		var singleObject = JsonNode.Parse(singleObjectJson);
		var patchedSingleObject = patchConfig.Apply(singleObject).Result;
		TestConsole.WriteLine(JsonSerializer.Serialize(patchedSingleObject, TestEnvironment.SerializerOptions));

		const string arrayObjectJson = "[" +
									   "{" +
									   "\"_id\":\"640729d45434f90313d25c78\"," +
									   "\"guid\":\"f2e2767c-03e0-4862-addc-7d46c55efb33\"," +
									   "\"first_name\":\"Kathrine\"," +
									   "\"last_name\":\"Pate\"" +
									   "}," +
									   "{\"_id\":\"640729d45b5824ffcabc30a5\"," +
									   "\"guid\":\"73193eda-074b-4f31-9f09-507a008ccb75\"," +
									   "\"first_name\":\"Rivers\"," +
									   "\"last_name\":\"Smith\"" +
									   "}" +
									   "]";

		var arrayObject = JsonNode.Parse(arrayObjectJson);

		var jsonArray = arrayObject!.AsArray();

		// Way 3: remove from initial array and then patch
		for (int currentIndex = jsonArray.Count - 1; currentIndex >= 0; currentIndex--)
		{
			var nodeToPatch = jsonArray[currentIndex];
			jsonArray.RemoveAt(currentIndex);

			var patchedNode = patchConfig.Apply(nodeToPatch).Result; // <-  throws an error
			TestConsole.WriteLine(JsonSerializer.Serialize(patchedNode, TestEnvironment.SerializerOptions));
		}
	}

	[Test]
	public void Issue397_ReplaceShouldThrowForMissingValue()
	{
		JsonNode? maskJson = "*****";

		var pathsToPatch = new[] { "/first_name", "/last_name" };

		var patchOperations = pathsToPatch.Select(path => PatchOperation.Replace(JsonPointer_Old.Parse(path), maskJson));
		var patchConfig = new JsonPatch(patchOperations);

		const string singleObjectJson = @"{
	""id"":""640729d45434f90313d25c78"",
    ""guid"":""f2e2767c-03e0-4862-addc-7d46c55efb33"",
	""city"":""Boston""
}";

		var singleObject = JsonNode.Parse(singleObjectJson);
		var result = patchConfig.Apply(singleObject);
		var serializerOptions = new JsonSerializerOptions
		{
			TypeInfoResolverChain = { TestSerializerContext.Default },
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};
		TestConsole.WriteLine(JsonSerializer.Serialize(result, serializerOptions));
		Assert.That(patchConfig.Apply(singleObject).Error, Is.Not.Null);
	}

	public class Target543
	{
		public List<int> Values { get; set; } = null!;
	}

	[Test]
	public void Issue543_CreatePatchToAddItem()
	{
		var targetObj = new Target543 { Values = [1, 2, 3, 4] };
		var target = JsonSerializer.SerializeToNode(targetObj, TestEnvironment.SerializerOptions);

		var jsonPointer = JsonPointer_Old.Create<Target543>(x => x.Values.Last());
		var jsonPatch = new JsonPatch(PatchOperation.Add(jsonPointer, (JsonNode)42));

		var expected = new JsonObject
		{
			["Values"] = new JsonArray(1, 2, 3, 4, 42)
		};

		var patchResult = jsonPatch.Apply(target);

		Assert.That(expected.IsEquivalentTo(patchResult.Result), Is.True);
	}

	[Test]
	public void Issue825_MoveShouldMoveItemToTheEndOfArray()
	{
		var jsonModel = new JsonObject
		{
			["Items"] = new JsonArray(1, 2, 3, 4, 5)
		};

		var doc = new JsonPatch(
			PatchOperation.Move(
				JsonPointer_Old.Parse("/Items/2"),
				JsonPointer_Old.Parse("/Items/-")));

		var result = doc.Apply(jsonModel);

		Assert.That(result.Error, Is.Null);

		var jsonArray = result.Result!["Items"]!.AsArray();
		Assert.That(jsonArray, Has.Count.EqualTo(5));
		Assert.That(jsonArray[^1].IsEquivalentTo(3));
	}

	[Test]
	public void Issue825_CopyShouldCopyItemToEndOfArray()
	{
		var jsonModel = new JsonObject
		{
			["Items"] = new JsonArray(1, 2, 3, 4, 5)
		};

		var doc = new JsonPatch(
			PatchOperation.Copy(
				JsonPointer_Old.Parse("/Items/2"),
				JsonPointer_Old.Parse("/Items/-")));

		var result = doc.Apply(jsonModel);

		Assert.That(result.Error, Is.Null);

		var jsonArray = result.Result!["Items"]!.AsArray();
		Assert.That(jsonArray, Has.Count.EqualTo(6));
		Assert.That(jsonArray[^1].IsEquivalentTo(3));
	}

	[Test]
	public void Issue825_RemoveShouldRemoveItemAtEndOfArray()
	{
		var jsonModel = new JsonObject
		{
			["Items"] = new JsonArray(1, 2, 3, 4, 5)
		};

		var doc = new JsonPatch(PatchOperation.Remove(JsonPointer_Old.Parse("/Items/-")));

		var result = doc.Apply(jsonModel);

		Assert.That(result.Error, Is.Null);

		var jsonArray = result.Result!["Items"]!.AsArray();
		Assert.That(jsonArray, Has.Count.EqualTo(4));
		Assert.That(jsonArray[^1].IsEquivalentTo(4));
	}

	[TestCase("/Items/-")]
	[TestCase("/Items/4")]
	public void Issue825_ReplaceShouldReplaceItemToEndOfArray(string path)
	{
		var jsonModel = new JsonObject
		{
			["Items"] = new JsonArray(1, 2, 3, 4, 5)
		};

		var doc = new JsonPatch(PatchOperation.Replace(
			JsonPointer_Old.Parse(path),
			JsonNode.Parse("6")));

		var result = doc.Apply(jsonModel);

		Assert.That(result.Error, Is.Null);

		var jsonArray = result.Result!["Items"]!.AsArray();
		Assert.That(jsonArray, Has.Count.EqualTo(5));
		Assert.That(jsonArray[^1].IsEquivalentTo(6));
	}

	[Test]
	public void Issue826_CopyShouldCopyValuesToTarget()
	{
		var jsonModel = new JsonObject
		{
			["Items"] = new JsonArray(1, 2, 3, 4, 5)
		};
		var expected = new JsonObject
		{
			["Items"] = new JsonArray(3, 1, 2, 3, 4, 5)
		};

		var addDoc = new JsonPatch(
			PatchOperation.Add(
				JsonPointer_Old.Parse("/Items/0"),
				JsonNode.Parse("3")));

		var addResult = addDoc.Apply(jsonModel);

		Assert.That(addResult.Error, Is.Null);
		Console.WriteLine(addResult.Result.AsJsonString());
		Assert.That(addResult.Result.IsEquivalentTo(expected));

		var copyDoc = new JsonPatch(
			PatchOperation.Copy(
				JsonPointer_Old.Parse("/Items/2"),
				JsonPointer_Old.Parse("/Items/0")));

		var copyResult = copyDoc.Apply(jsonModel);

		Assert.That(copyResult.Error, Is.Null);
		Console.WriteLine(copyResult.Result.AsJsonString());
		Assert.That(copyResult.Result.IsEquivalentTo(expected));
	}
}