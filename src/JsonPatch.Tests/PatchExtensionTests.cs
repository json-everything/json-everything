﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using NUnit.Framework;
using TestHelpers;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Json.Patch.Tests;

public class PatchExtensionTests
{
	internal class TestModel
	{
		public Guid Id { get; set; }
		public string? Name { get; set; }
		public int[]? Numbers { get; set; }
		public string[]? Strings { get; set; }
		public List<TestModel>? InnerObjects { get; set; }
		public JsonNode? Attributes { get; set; }
	}

	[Test]
	public void CreatePatch()
	{
		var initial = new TestModel
		{
			Id = Guid.NewGuid(),
			Attributes = JsonNode.Parse("[{\"test\":\"test123\"},{\"test\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,4]}]")
		};
		var expected = new TestModel
		{
			Id = Guid.Parse("40664cc7-864f-4eed-939c-78076a252df0"),
			Attributes = JsonNode.Parse("[{\"test\":\"test123\"},{\"test\":\"test32132\"},{\"test1\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,3]}]")
		};
		var patchExpected = JsonNode.Parse(
			"[{\"op\":\"replace\",\"path\":\"/Id\",\"value\":\"40664cc7-864f-4eed-939c-78076a252df0\"}," +
			"{\"op\":\"replace\",\"path\":\"/Attributes/1/test\",\"value\":\"test32132\"}," +
			"{\"op\":\"remove\",\"path\":\"/Attributes/2/test\"}," +
			"{\"op\":\"add\",\"path\":\"/Attributes/2/test1\",\"value\":\"test321\"}," +
			"{\"op\":\"replace\",\"path\":\"/Attributes/3/test/2\",\"value\":3}," +
			"{\"op\":\"add\",\"path\":\"/Attributes/4\",\"value\":{\"test\":[1,2,3]}}]");

		var patch = initial.CreatePatch(expected, TestEnvironment.SerializerOptions);

		var serialized = JsonSerializer.SerializeToNode(patch, TestEnvironment.SerializerOptions);

		Assert.That(serialized.IsEquivalentTo(patchExpected), Is.True);
	}

	[Test]
	public void CreatePatch2()
	{
		var initial = JsonNode.Parse("[{\"test\":\"test123\"},{\"test\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,4]}]");
		var expected = JsonNode.Parse("[{\"test\":\"test123\"},{\"test\":\"test32132\"},{\"test1\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,3]}]");
		var patchExpected = JsonSerializer.Deserialize<JsonPatch>(
			"[{\"op\":\"replace\",\"path\":\"/1/test\",\"value\":\"test32132\"},{\"op\":\"remove\",\"path\":\"/2/test\"},{\"op\":\"add\",\"path\":\"/2/test1\",\"value\":\"test321\"},{\"op\":\"replace\",\"path\":\"/3/test/2\",\"value\":3},{\"op\":\"add\",\"path\":\"/4\",\"value\":{\"test\":[1,2,3]}}]",
			TestEnvironment.SerializerOptions
		);

		var patch = initial.CreatePatch(expected, TestEnvironment.SerializerOptions);

		VerifyPatches(patchExpected!, patch);
	}

	[Test]
	public void CreatePatch_ChangeTypeInArray()
	{
		var initial = JsonNode.Parse("[{\"test\":true},{\"test\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,4]},{\"test\":[1,2,3]}]");
		var expected = JsonNode.Parse("[{\"test\":false},{\"test\":\"test32132\"},{\"test1\":\"test321\"},{\"test\":[1,2,3]},{\"test\":{\"test\":123}},{\"test\":[1,2,3]}]");
		var patchExpected = JsonSerializer.Deserialize<JsonPatch>(
			"[{\"op\":\"replace\",\"path\":\"/0/test\",\"value\":false},{\"op\":\"replace\",\"path\":\"/1/test\",\"value\":\"test32132\"},{\"op\":\"remove\",\"path\":\"/2/test\"},{\"op\":\"add\",\"path\":\"/2/test1\",\"value\":\"test321\"},{\"op\":\"replace\",\"path\":\"/3/test/2\",\"value\":3},{\"op\":\"replace\",\"path\":\"/4/test\",\"value\":{\"test\":123}},{\"op\":\"add\",\"path\":\"/5\",\"value\":{\"test\":[1,2,3]}}]",
			TestEnvironment.SerializerOptions
		);

		var patch = initial.CreatePatch(expected, TestEnvironment.SerializerOptions);

		VerifyPatches(patchExpected!, patch);
	}

	[Test]
	public void CreatePatch_ChangeTypeInObject()
	{
		var initial = JsonNode.Parse("{\"test\":true, \"test2\":\"string\", \"test3\":{\"test123\":123}}");
		var expected = JsonNode.Parse("{\"test\":false, \"test2\":123, \"test3\":[123]}");
		var patchExpected = JsonSerializer.Deserialize<JsonPatch>(
			"[{\"op\":\"replace\",\"path\":\"/test\",\"value\":false},{\"op\":\"replace\",\"path\":\"/test2\",\"value\":123},{\"op\":\"replace\",\"path\":\"/test3\",\"value\":[123]}]",
			TestEnvironment.SerializerOptions
		);

		var patch = initial.CreatePatch(expected, TestEnvironment.SerializerOptions);

		VerifyPatches(patchExpected!, patch);
	}

	private static readonly JsonSerializerOptions _ignoreWritingNullSerializerOptions =
		new()
		{
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				TypeInfoResolverChain = { TestSerializerContext.Default }
			};

	[Test]
	public void Add()
	{
		var initial = new TestModel
		{
			Id = Guid.NewGuid()
		};
		var target = new TestModel
		{
			Id = initial.Id,
			Attributes = JsonNode.Parse("[{\"test\":\"test123\"},{\"test\":\"test32132\"},{\"test1\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,3]}]")
		};
		var patchExpectedStr = "[{\"op\":\"add\",\"path\":\"/Attributes\",\"value\":[{\"test\":\"test123\"},{\"test\":\"test32132\"},{\"test1\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,3]}]}]";
		var expected = JsonSerializer.Deserialize<JsonPatch>(patchExpectedStr, TestEnvironment.SerializerOptions)!;
		var patch = initial.CreatePatch(target, _ignoreWritingNullSerializerOptions);

		VerifyPatches(expected, patch);
	}

	[Test]
	public void Remove()
	{
		var initial = new TestModel
		{
			Id = Guid.NewGuid(),
			Attributes = JsonNode.Parse("[{\"test\":\"test123\"},{\"test\":\"test32132\"},{\"test1\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,3]}]")
		};
		var expected = new TestModel
		{
			Id = initial.Id
		};
		var patchExpectedStr = "[{\"op\":\"remove\",\"path\":\"/Attributes\"}]";
		var patchExpected = JsonSerializer.Deserialize<JsonPatch>(patchExpectedStr, TestEnvironment.SerializerOptions)!;

		var patch = initial.CreatePatch(expected, _ignoreWritingNullSerializerOptions);

		VerifyPatches(patchExpected, patch);
	}

	[Test]
	public void Replace()
	{
		var initial = new TestModel
		{
			Id = Guid.NewGuid()
		};
		var expected = new TestModel
		{
			Id = Guid.Parse("a299e216-dbbe-40e4-b4d4-556d7e7e9c35")
		};
		var patchExpectedStr = "[{\"op\":\"replace\",\"path\":\"/Id\",\"value\":\"a299e216-dbbe-40e4-b4d4-556d7e7e9c35\"}]";
		var patchExpected = JsonSerializer.Deserialize<JsonPatch>(patchExpectedStr, TestEnvironment.SerializerOptions);
		var patch = initial.CreatePatch(expected, TestEnvironment.SerializerOptions);

		VerifyPatches(patchExpected!, patch);
	}

	[Test]
	public void AddArray()
	{
		var initial = JsonNode.Parse("[1,2,3]");
		var expected = JsonNode.Parse("[1,2,3,4]");
		var patchExpectedStr = "[{\"op\":\"add\",\"path\":\"/3\",\"value\":4}]";
		var patchExpected = JsonSerializer.Deserialize<JsonPatch>(patchExpectedStr, TestEnvironment.SerializerOptions);
		var patch = initial.CreatePatch(expected, TestEnvironment.SerializerOptions);

		VerifyPatches(patchExpected!, patch);
	}

	[Test]
	public void RemoveArray()
	{
		var initial = JsonNode.Parse("[1,2,3]");
		var expected = JsonNode.Parse("[1,2]");
		var patchExpectedStr = "[{\"op\":\"remove\",\"path\":\"/2\"}]";
		var patchExpected = JsonSerializer.Deserialize<JsonPatch>(patchExpectedStr, TestEnvironment.SerializerOptions);
		var patch = initial.CreatePatch(expected, TestEnvironment.SerializerOptions);

		VerifyPatches(patchExpected!, patch);
	}

	[Test]
	public void ReplaceArray()
	{
		var initial = JsonNode.Parse("[1,2,3]");
		var expected = JsonNode.Parse("[1,2,1]");
		var patch = initial.CreatePatch(expected);
		var patchExpectedStr = "[{\"op\":\"replace\",\"path\":\"/2\",\"value\":1}]";
		var patchExpected = JsonSerializer.Deserialize<JsonPatch>(patchExpectedStr, TestEnvironment.SerializerOptions);

		VerifyPatches(patchExpected!, patch);
	}

	[Test]
	public void ComplexObject()
	{
		var initial = new TestModel
		{
			Id = Guid.Parse("aa7daced-c9fa-489b-9bc1-540b21d277a1"),
			Attributes = JsonNode.Parse("[{\"test\":\"test123\"},{\"test\":\"test32132\"},{\"test1\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,3]}]"),
			Name = "Test",
			Numbers = [1, 2, 3],
			Strings = ["test1", "test2"],
			InnerObjects =
			[
				new()
				{
					Id = Guid.Parse("b2cab2a0-ec23-405a-a5a8-975448a10334"),
					Name = "TestNameInner1",
					Numbers = [3, 2, 1],
					Strings = ["Test3", "test4"]
				}
			]
		};
		var expected = new TestModel
		{
			Id = Guid.Parse("4801bd62-a8ec-4ef2-ae3c-52b9f541625f"),
			Attributes = JsonNode.Parse("[{\"test1\":\"test123\"},{\"test\":\"test32132\"},{\"test1\":\"test321\"},{\"test\":[1,1,3]}]"),
			Name = "Test4",
			Numbers = [1, 2, 3, 4],
			Strings = ["test2", "test2"],
			InnerObjects =
			[
				new()
				{
					Id = Guid.Parse("bed584b0-7ccc-4336-adba-d0d7f7c3c3f2"),
					Name = "TestNameInner1",
					Numbers = [1, 2, 1],
					Strings = ["Test3", "test4", "test5"]
				}
			]
		};
		var patchExpectedStr =
			"[{\"op\":\"replace\",\"path\":\"/Id\",\"value\":\"4801bd62-a8ec-4ef2-ae3c-52b9f541625f\"}," +
			"{\"op\":\"replace\",\"path\":\"/Name\",\"value\":\"Test4\"}," +
			"{\"op\":\"add\",\"path\":\"/Numbers/3\",\"value\":4}," +
			"{\"op\":\"replace\",\"path\":\"/Strings/0\",\"value\":\"test2\"}," +
			"{\"op\":\"replace\",\"path\":\"/InnerObjects/0/Id\",\"value\":\"bed584b0-7ccc-4336-adba-d0d7f7c3c3f2\"}," +
			"{\"op\":\"replace\",\"path\":\"/InnerObjects/0/Numbers/0\",\"value\":1}," +
			"{\"op\":\"add\",\"path\":\"/InnerObjects/0/Strings/2\",\"value\":\"test5\"}," +
			"{\"op\":\"remove\",\"path\":\"/Attributes/0/test\"}," +
			"{\"op\":\"add\",\"path\":\"/Attributes/0/test1\",\"value\":\"test123\"}," +
			"{\"op\":\"replace\",\"path\":\"/Attributes/3/test/1\",\"value\":1}," +
			"{\"op\":\"remove\",\"path\":\"/Attributes/4\"}]";
		var patchExpected = JsonSerializer.Deserialize<JsonPatch>(patchExpectedStr, TestEnvironment.SerializerOptions);

		var patchBackExpectedStr =
			"[{\"op\":\"replace\",\"path\":\"/Id\",\"value\":\"aa7daced-c9fa-489b-9bc1-540b21d277a1\"}," +
			"{\"op\":\"replace\",\"path\":\"/Name\",\"value\":\"Test\"}," +
			"{\"op\":\"remove\",\"path\":\"/Numbers/3\"}," +
			"{\"op\":\"replace\",\"path\":\"/Strings/0\",\"value\":\"test1\"}," +
			"{\"op\":\"replace\",\"path\":\"/InnerObjects/0/Id\",\"value\":\"b2cab2a0-ec23-405a-a5a8-975448a10334\"}," +
			"{\"op\":\"replace\",\"path\":\"/InnerObjects/0/Numbers/0\",\"value\":3}," +
			"{\"op\":\"remove\",\"path\":\"/InnerObjects/0/Strings/2\"}," +
			"{\"op\":\"remove\",\"path\":\"/Attributes/0/test1\"}," +
			"{\"op\":\"add\",\"path\":\"/Attributes/0/test\",\"value\":\"test123\"}," +
			"{\"op\":\"replace\",\"path\":\"/Attributes/3/test/1\",\"value\":2},{\"op\":\"add\",\"path\":\"/Attributes/4\",\"value\":{\"test\":[1,2,3]}}]";
		var patchBackExpected = JsonSerializer.Deserialize<JsonPatch>(patchBackExpectedStr, TestEnvironment.SerializerOptions);

		var patch = initial.CreatePatch(expected, TestEnvironment.SerializerOptions);
		var patchBack = expected.CreatePatch(initial, TestEnvironment.SerializerOptions);

		VerifyPatches(patchExpected!, patch);
		VerifyPatches(patchBackExpected!, patchBack);
	}

	[Test]
	public void CreatePatch_ClearArray()
	{
		var model = new TestModel
		{
			Numbers = [1, 2, 1],
			Strings = ["asdf "],
			InnerObjects = [new() { Id = Guid.NewGuid() }, new() { Id = Guid.NewGuid() }]
		};
		var model2 = new TestModel
		{
			Numbers = [],
			Strings = [],
			InnerObjects = []
		};

		var patch = model.CreatePatch(model2, TestEnvironment.SerializerOptions);

		var final = patch.Apply(model, TestEnvironment.SerializerOptions);

		Assert.Multiple(() =>
		{
			Assert.That(final!.Numbers!, Is.Empty);
			Assert.That(final.Strings!, Is.Empty);
			Assert.That(final.InnerObjects!, Is.Empty);
		});
	}

	[Test]
	public void CreatePatch_RemoveArrayItem()
	{
		var model = new TestModel
		{
			Numbers = [1, 2, 3],
			Strings = ["123", "asdf"],
			InnerObjects = [new() { Id = Guid.NewGuid() }, new() { Id = Guid.NewGuid() }]
		};
		var model2 = new TestModel
		{
			Numbers = [1, 3],
			Strings = ["asdf"],
			InnerObjects = [model.InnerObjects[1]]
		};

		var patch = model.CreatePatch(model2, TestEnvironment.SerializerOptions);

		var final = patch.Apply(model, TestEnvironment.SerializerOptions);

		Assert.Multiple(() =>
		{
			Assert.That(final!.Numbers!, Has.Length.EqualTo(2));
			Assert.That(final.Numbers![0], Is.EqualTo(1));
			Assert.That(final.Numbers[1], Is.EqualTo(3));

			Assert.That(final.Strings!, Has.Length.EqualTo(1));
			Assert.That(final.Strings![0], Is.EqualTo("asdf"));

			Assert.That(final.InnerObjects!, Has.Count.EqualTo(1));
			Assert.That(final.InnerObjects![0].Id, Is.EqualTo(model.InnerObjects[1].Id));
		});
	}

	[Test]
	public void ApplyPatch_Respect_SerializationOptions()
	{
		var model = new TestModel
		{
			Numbers = [],
		};

		var patchStr = "[{\"op\":\"add\",\"path\":\"/numbers/-\",\"value\":5}]";
		var patch = JsonSerializer.Deserialize<JsonPatch>(patchStr, TestEnvironment.SerializerOptions)!;

		var options = new JsonSerializerOptions
		{
			TypeInfoResolverChain = { TestSerializerContext.Default },
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};
		var final = patch.Apply(model, options);

		Assert.That(final?.Numbers?[0], Is.EqualTo(5));
	}

	[Test]
	public void CreatePatch_JsonContext()
	{
		var initial = new TestModel
		{
			Id = Guid.NewGuid(),
			Attributes = JsonNode.Parse("[{\"test\":\"test123\"},{\"test\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,4]}]")
		};
		var expected = new TestModel
		{
			Id = Guid.Parse("40664cc7-864f-4eed-939c-78076a252df0"),
			Attributes = JsonNode.Parse("[{\"test\":\"test123\"},{\"test\":\"test32132\"},{\"test1\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,3]}]")
		};
		var patchExpected = JsonNode.Parse(
			"[{\"op\":\"replace\",\"path\":\"/Id\",\"value\":\"40664cc7-864f-4eed-939c-78076a252df0\"}," +
			"{\"op\":\"replace\",\"path\":\"/Attributes/1/test\",\"value\":\"test32132\"}," +
			"{\"op\":\"remove\",\"path\":\"/Attributes/2/test\"}," +
			"{\"op\":\"add\",\"path\":\"/Attributes/2/test1\",\"value\":\"test321\"}," +
			"{\"op\":\"replace\",\"path\":\"/Attributes/3/test/2\",\"value\":3}," +
			"{\"op\":\"add\",\"path\":\"/Attributes/4\",\"value\":{\"test\":[1,2,3]}}]");

		var patch = initial.CreatePatch(expected, TestEnvironment.SerializerOptions);
		// use source generated json serializer context
		var patchJson = JsonSerializer.SerializeToNode(patch, TestEnvironment.SerializerOptions);

		JsonAssert.AreEquivalent(patchExpected, patchJson);
	}

	[Test]
	public void SlashInPropertyName()
	{
		var initial = JsonNode.Parse("""
			{
			  "spec": {
			    "replicas": 0,
			    "selector": {
			      "matchLabels": {
			        "app": "myapp"
			      }
			    },
			    "template": {
			      "metadata": {
			        "creationTimestamp": null,
			        "labels": {
			          "app": "myapp"
			        },
			        "annotations": {
			          "kubectl.kubernetes.io/restartedAt": "1719861246",
			          "date": "1719861246"
			        }
			      }
			    }
			  }
			}
			""");
		var final = JsonNode.Parse("""
			{
			  "spec": {
			    "replicas": 0,
			    "selector": {
			      "matchLabels": {
			        "app": "myapp"
			      }
			    },
			    "template": {
			      "metadata": {
			        "creationTimestamp": null,
			        "labels": {
			          "app": "myapp"
			        },
			        "annotations": {
			          "kubectl.kubernetes.io/restartedAt": "",
			          "date": "1719925004"
			        }
			      }
			    }
			  }
			}
			""");
		var expected = JsonSerializer.Deserialize<JsonPatch>(
			"""
			[
			  {
			    "op": "replace",
			    "path": "/spec/template/metadata/annotations/kubectl.kubernetes.io~1restartedAt",
			    "value": ""
			  },
			  {
			    "op": "replace",
			    "path": "/spec/template/metadata/annotations/date",
			    "value": "1719925004"
			  }
			]
			""",
			TestEnvironment.SerializerOptions
		);

		var actual = initial.CreatePatch(final, TestEnvironment.SerializerOptions);

		VerifyPatches(expected!, actual);
	}

	private static readonly JsonSerializerOptions _indentedSerializerOptions =
		new()
		{
			TypeInfoResolverChain = { TestSerializerContext.Default },
			WriteIndented = true
		};


	private static void OutputPatch(JsonPatch patch)
	{
		TestConsole.WriteLine(JsonSerializer.Serialize(patch, _indentedSerializerOptions));
	}

	private static void VerifyPatches(JsonPatch expected, JsonPatch actual)
	{
		OutputPatch(expected);
		OutputPatch(actual);

		Assert.That(actual, Is.EqualTo(expected));
	}
}