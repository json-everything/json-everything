using System;
using System.Collections.Generic;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Generation.Tests
{
	public class ClientTests
	{
		public class TestMenu
		{
			public string Name { get; set; }

			public List<TestMenu> Children { get; set; }
		}

		[Test]
		public void Issue85_RecursiveGeneration_PropertyAsListOfSelf()
		{
			JsonSchema expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.Object)
				.Properties(
					(nameof(TestMenu.Name), new JsonSchemaBuilder()
						.Type(SchemaValueType.Array)
						.Items(new JsonSchemaBuilder().Ref("#")))
				);

			JsonSchema actual = new JsonSchemaBuilder().FromType<TestMenu>();

			Console.WriteLine(JsonSerializer.Serialize(expected, new JsonSerializerOptions { WriteIndented = true }));
			Console.WriteLine(JsonSerializer.Serialize(actual, new JsonSerializerOptions { WriteIndented = true }));
			Assert.AreEqual(expected, actual);
		}
	}
}