using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Json.Schema.Generation.Intents;
using NUnit.Framework;

using static Json.Schema.Generation.Tests.AssertionExtensions;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global

namespace Json.Schema.Generation.Tests;

public class ClientTests
{
	[UsedImplicitly]
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

	[UsedImplicitly]
	public class TreeNode
	{
		public string Value { get; set; }

		[JsonPropertyName("left")] public TreeNodeMetaData Left { get; set; }
		[JsonPropertyName("right")] public TreeNodeMetaData Right { get; set; }
	}

	[UsedImplicitly]
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

		Console.WriteLine(JsonSerializer.Serialize(expected, new JsonSerializerOptions { WriteIndented = true }));
		Console.WriteLine(JsonSerializer.Serialize(actual, new JsonSerializerOptions { WriteIndented = true }));
		Assert.AreEqual(expected, actual);
	}

	[UsedImplicitly]
	private class SimpleValueWidgetSettings
	{
		[Required] public string name { get; set; }

		[Required] public string valueLink { get; set; }

		[Required] public string qualityValueLink { get; set; }

		[Required] public string unitLink { get; set; }

		[Required] public string displayMultiplier { get; set; }

		[Required] public string displayUnits { get; set; }

		[Required] public string fontColor { get; set; }

		[Required] public string stringFormat { get; set; }
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

	[AttributeUsage(AttributeTargets.Property)]
	private class UnfortunateAttribute : Attribute
	{
	}

	[UsedImplicitly]
	private class ObjectA
	{
		public Guid Property1 { get; set; }
		[Unfortunate]
		public Guid Property2 { get; set; }
	}

	[Test]
	public void Issue272_UnhandledAttributeShouldBeIgnoredWhenOptimizing()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Property1", new JsonSchemaBuilder().Ref("#/$defs/Guid")),
				("Property2", new JsonSchemaBuilder().Ref("#/$defs/Guid"))
			)
			.Defs(
				("Guid", new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.Format(Formats.Uuid)
				)
			);

		JsonSchema actual = new JsonSchemaBuilder().FromType<ObjectA>();

		AssertEqual(expected, actual);
	}

	private class JsonObjectProp
	{
		public JsonObject ObjectProp { get; set; }
		public JsonArray ArrayProp { get; set; }
		public JsonValue ValueProp { get; set; }
	}

	[Test]
	public void GenerateForPlanImpl()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("ObjectProp", new JsonSchemaBuilder().Type(SchemaValueType.Object)),
				("ArrayProp", new JsonSchemaBuilder().Type(SchemaValueType.Array)),
				("ValueProp", true)
			);

		JsonSchema actual = new JsonSchemaBuilder().FromType<JsonObjectProp>();

		AssertEqual(expected, actual);
	}

	public class Person
	{
		[MyAttribute1]
		public string FirstName { get; set; }
		[MyAttribute2]
		public string LastName { get; set; }
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class MyAttribute1 : Attribute { }

	[AttributeUsage(AttributeTargets.Property)]
	public class MyAttribute2 : Attribute { }

	public class PersonRefiner : ISchemaRefiner
	{
		public Dictionary<string, List<Type>> FoundAttributes { get; } = new();

		public void Run(SchemaGenerationContextBase context)
		{
			var properties = context.Intents.OfType<PropertiesIntent>().Single();
			var firstNameContext = (MemberGenerationContext)properties.Properties["FirstName"];
			var lastNameContext = (MemberGenerationContext)properties.Properties["LastName"];

			FoundAttributes[nameof(Person.FirstName)] = firstNameContext.Attributes.Select(x => x.GetType()).ToList();
			FoundAttributes[nameof(Person.LastName)] = lastNameContext.Attributes.Select(x => x.GetType()).ToList();
		}

		public bool ShouldRun(SchemaGenerationContextBase context)
		{
			return context.Type == typeof(Person);
		}
	}

	[Test]
	[Ignore("Works as expected.  See issue.")]
	public void Issue277_AttributeMixup()
	{
		JsonSchemaBuilder jsonSchemaBuilder = new();
		SchemaGeneratorConfiguration generatorConfiguration = new();
		var personRefiner = new PersonRefiner();
		generatorConfiguration.Refiners.Add(personRefiner);
		jsonSchemaBuilder.FromType(typeof(Person), generatorConfiguration).Build();

		Assert.AreEqual(1, personRefiner.FoundAttributes[nameof(Person.FirstName)].Count);
		Assert.IsTrue(personRefiner.FoundAttributes[nameof(Person.FirstName)][0] == typeof(MyAttribute1));
		Assert.AreEqual(1, personRefiner.FoundAttributes[nameof(Person.LastName)].Count);
		Assert.IsTrue(personRefiner.FoundAttributes[nameof(Person.LastName)][0] == typeof(MyAttribute2));
	}
}