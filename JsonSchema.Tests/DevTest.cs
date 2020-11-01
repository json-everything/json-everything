using System;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests
{
	public class DevTest
	{
		[Test]
		public void Test()
		{
			var schema = JsonSchema.FromText(@"");
			var instance = JsonDocument.Parse(@"").RootElement;

			var results = schema.Validate(instance);

			Console.WriteLine(JsonSerializer.Serialize(results));
		}
	}
}
