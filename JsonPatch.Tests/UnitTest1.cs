using System.Text.Json;
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
			Assert.IsNotNull(patch);
		}
	}
}