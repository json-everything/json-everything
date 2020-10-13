using System;
using System.Linq;
using System.Text.Json;
using Json.More;
using Json.Patch;
using NUnit.Framework;

namespace JsonPatch.Tests
{
	public class Tests
	{
		[Test]
		public void Test1()
		{
			var patch = JsonSerializer.Deserialize<Json.Patch.JsonPatch>(@"[
  { ""op"": ""replace"", ""path"": ""/baz"", ""value"": ""boo"" },
  { ""op"": ""add"", ""path"": ""/hello"", ""value"": [""world""] },
  { ""op"": ""remove"", ""path"": ""/foo"" }
]");
			Assert.AreEqual(3, patch.Operations.Count);
			Assert.AreEqual(OperationType.Replace, patch.Operations[0].Op);
			Assert.AreEqual(OperationType.Add, patch.Operations[1].Op);
			Assert.AreEqual(OperationType.Remove, patch.Operations[2].Op);
		}

		[Test]
		public void ObjectModify()
		{
			// insert false into key "two"
			var element = JsonDocument.Parse("{\"one\":[1,2,3]}").RootElement;

			var dict = element.EnumerateObject().ToDictionary(kvp => kvp.Name, kvp => kvp.Value);
			dict["two"] = false.AsJsonElement();

			element = dict.AsJsonElement();

			Console.WriteLine(element.ToJsonString());
		}
	}
}