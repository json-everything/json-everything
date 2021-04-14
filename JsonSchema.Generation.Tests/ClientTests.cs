using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
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
						.Type(SchemaValueType.String)
					),
					(nameof(TestMenu.Children), new JsonSchemaBuilder()
						.Type(SchemaValueType.Array)
						.Items(new JsonSchemaBuilder().Ref("#"))
					)
				);

			VerifyGeneration<TestMenu>(expected);
		}

		public class TreeNode
		{
			public string Value { get; set; }

			[JsonPropertyName("left")]
			public TreeNodeMetaData Left { get; set; }
			[JsonPropertyName("right")]
			public TreeNodeMetaData Right { get; set; }
		}

		public class TreeNodeMetaData
		{
			public TreeNode Node { get; set; }
			public int Position { get; set; }
		}

		[Test]
		public void Issue87_RecursiveGeneration_SelfReferenceInDefs()
		{
			JsonSchema expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.Object)
				.Defs(
					(nameof(TreeNodeMetaData), new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.Properties(
							(nameof(TreeNodeMetaData.Node), new JsonSchemaBuilder().Ref("#")),
							(nameof(TreeNodeMetaData.Position), new JsonSchemaBuilder().Type(SchemaValueType.Integer))
						)
					)
				)
				.Properties(
					(nameof(TreeNode.Value), new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
					),
					("left", new JsonSchemaBuilder()
						.Ref($"#/$defs/{nameof(TreeNodeMetaData)}")
					),
					("right", new JsonSchemaBuilder()
						.Ref($"#/$defs/{nameof(TreeNodeMetaData)}")
					)
				);

			VerifyGeneration<TreeNode>(expected);
		}

		private static void VerifyGeneration<T>(JsonSchema expected)
		{
			JsonSchema actual = new JsonSchemaBuilder().FromType<T>();

			Console.WriteLine(JsonSerializer.Serialize(expected, new JsonSerializerOptions {WriteIndented = true}));
			Console.WriteLine(JsonSerializer.Serialize(actual, new JsonSerializerOptions {WriteIndented = true}));
			Assert.AreEqual(expected, actual);
		}

		private class SimpleValueWidgetSettings
		{
			[Required]
			public string name { get; set; }

			[Required]
			public string valueLink { get; set; }

			[Required]
			public string qualityValueLink { get; set; }

			[Required]
			public string unitLink { get; set; }

			[Required]
			public string displayMultiplier { get; set; }

			[Required]
			public string displayUnits { get; set; }

			[Required]
			public string fontColor { get; set; }

			[Required]
			public string stringFormat { get; set; }
		}

		[Test]
		public void Issue97_PersistentDataBetweenGenerationCalls()
		{
			var simpleValueSettingsSchema1 = new JsonSchemaBuilder().FromType<SimpleValueWidgetSettings>().AdditionalProperties(false).Build();
			var simpleValueSettingsSchema2 = new JsonSchemaBuilder().FromType<SimpleValueWidgetSettings>().AdditionalProperties(false).Build();

			Console.WriteLine(JsonSerializer.Serialize(simpleValueSettingsSchema1, new JsonSerializerOptions { WriteIndented = true }));
			Console.WriteLine(JsonSerializer.Serialize(simpleValueSettingsSchema2, new JsonSerializerOptions { WriteIndented = true }));
			Assert.AreEqual(simpleValueSettingsSchema1, simpleValueSettingsSchema2);
		}
	}
}