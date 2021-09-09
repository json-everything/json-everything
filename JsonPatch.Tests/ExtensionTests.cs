using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Patch;
using Json.Schema;
using NUnit.Framework;
using NUnit.Framework.Internal.Execution;

namespace JsonPatch.Tests
{
	public class ExtensionTests
	{
		[Test]
		public void CreatePatch_Test()
		{
			var e1 = new TestModel()
			{
				Id = Guid.NewGuid(),
				Attributes = JsonDocument.Parse("[{\"test\":\"test123\"},{\"test\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,4]}]")
			};
			var e2 = new TestModel()
			{
				Id = Guid.NewGuid(),
				Attributes = JsonDocument.Parse("[{\"test\":\"test123\"},{\"test\":\"test32132\"},{\"test1\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,3]}]")
			};
			var op = e1.CreatePatch(e2);
			Assert.AreEqual(op.Operations.Count, 6);

			Assert.AreNotEqual(e1.Id, e2.Id);
			Assert.AreNotEqual(e1.Attributes.RootElement.ToString(), e2.Attributes.RootElement.ToString());

			e1 = op.ApplyPatch(e1);

			Assert.AreEqual(e1.Id, e2.Id);
			Assert.AreEqual(e1.Attributes.RootElement.ToString(), e2.Attributes.RootElement.ToString());
		}

		[Test]
		public void Add_Test()
		{
			var e1 = new TestModel()
			{
				Id = Guid.NewGuid()
			};
			var e2 = new TestModel()
			{
				Id = e1.Id,
				Attributes = JsonDocument.Parse("[{\"test\":\"test123\"},{\"test\":\"test32132\"},{\"test1\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,3]}]")
			};
			var op = e1.CreatePatch(e2, new JsonSerializerOptions() {IgnoreNullValues = true});
			Assert.AreEqual(op.Operations.Count, 1);
			Assert.AreEqual(op.Operations[0].Op, OperationType.Add);

			Assert.Null(e1.Attributes);
			e1 = op.ApplyPatch(e1);
			Assert.AreEqual(e1.Attributes.RootElement.ToString(), e2.Attributes.RootElement.ToString());
		}

		[Test]
		public void Remove_Test()
		{
			var e1 = new TestModel()
			{
				Id = Guid.NewGuid(),
				Attributes = JsonDocument.Parse("[{\"test\":\"test123\"},{\"test\":\"test32132\"},{\"test1\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,3]}]")
			};
			var e2 = new TestModel()
			{
				Id = e1.Id
			};
			var op = e1.CreatePatch(e2, new JsonSerializerOptions() {IgnoreNullValues = true});
			Assert.AreEqual(op.Operations.Count, 1);
			Assert.AreEqual(op.Operations[0].Op, OperationType.Remove);

			Assert.NotNull(e1.Attributes);
			e1 = op.ApplyPatch(e1);
			Assert.Null(e1.Attributes);
		}

		[Test]
		public void Replace_Test()
		{
			var e1 = new TestModel()
			{
				Id = Guid.NewGuid()
			};
			var e2 = new TestModel()
			{
				Id = Guid.NewGuid()
			};
			var op = e1.CreatePatch(e2);
			Assert.AreEqual(op.Operations.Count, 1);
			Assert.AreEqual(op.Operations[0].Op, OperationType.Replace);

			Assert.AreNotEqual(e1.Id, e2.Id);
			e1 = op.ApplyPatch(e1);
			Assert.AreEqual(e1.Id, e2.Id);
		}


		[Test]
		public void ComplexObject_Test()
		{
			var e1 = new TestModel()
			{
				Id = Guid.NewGuid(),
				Attributes = JsonDocument.Parse("[{\"test\":\"test123\"},{\"test\":\"test32132\"},{\"test1\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,3]}]"),
				Name = "Test",
				Numbers = new[] {1, 2, 3},
				Strings = new[] {"test1", "test2"},
				InnerObjects = new List<TestModel>() {new TestModel() {Id = Guid.NewGuid(), Name = "TestNameInner1", Numbers = new[] {3, 2, 1}, Strings = new[] {"Test3", "test4"}}}
			};
			var e2 = new TestModel()
			{
				Id = Guid.NewGuid(),
				Attributes = JsonDocument.Parse("[{\"test1\":\"test123\"},{\"test\":\"test32132\"},{\"test1\":\"test321\"},{\"test\":[1,1,3]}]"),
				Name = "Test4",
				Numbers = new[] {1, 2, 3, 4},
				Strings = new[] {"test2", "test2"},
				InnerObjects = new List<TestModel>() {new TestModel() {Id = Guid.NewGuid(), Name = "TestNameInner1", Numbers = new[] {1, 2, 1}, Strings = new[] {"Test3", "test4", "test5"}}}
			};
			var op = e1.CreatePatch(e2, new JsonSerializerOptions() {IgnoreNullValues = true});

			Assert.AreNotEqual(e1, e2);
			e1 = op.ApplyPatch(e1);
			Assert.AreEqual(e1, e2);
		}
	}

	public static class Extensions
	{
		public static T ApplyPatch<T>(this Json.Patch.JsonPatch patch, T obj)
		{
			using var doc = JsonDocument.Parse(JsonSerializer.Serialize(obj));
			var res = patch.Apply(doc.RootElement).Result;
			var result = JsonSerializer.Deserialize<T>(res.GetRawText());
			return result;
		}
	}

	public class TestModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public int[] Numbers { get; set; }
		public string[] Strings { get; set; }
		public List<TestModel> InnerObjects { get; set; }
		public JsonDocument Attributes { get; set; }

		public override bool Equals(object? obj)
		{
			if (obj is TestModel m)
			{
				return JsonSerializer.Serialize(this) == JsonSerializer.Serialize(m);

			}

			return false;
		}

	}
}