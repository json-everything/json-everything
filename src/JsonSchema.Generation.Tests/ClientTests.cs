using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Schema.Generation.Intents;
using NUnit.Framework;
using TestHelpers;
using static Json.Schema.Generation.Tests.AssertionExtensions;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Json.Schema.Generation.Tests;

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

		[JsonPropertyName("left")] public TreeNodeMetaData Left { get; set; }
		[JsonPropertyName("right")] public TreeNodeMetaData Right { get; set; }
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
				("treeNodeMetaDataInClientTests", new JsonSchemaBuilder()
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
					.Ref($"#/$defs/treeNodeMetaDataInClientTests")
				),
				("right", new JsonSchemaBuilder()
					.Ref($"#/$defs/treeNodeMetaDataInClientTests")
				)
			);

		VerifyGeneration<TreeNode>(expected);
	}

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

		TestConsole.WriteLine(JsonSerializer.Serialize(simpleValueSettingsSchema1, TestSerializerContext.Default.JsonSchema));
		TestConsole.WriteLine(JsonSerializer.Serialize(simpleValueSettingsSchema2, TestSerializerContext.Default.JsonSchema));
		AssertEqual(simpleValueSettingsSchema1, simpleValueSettingsSchema2);
	}

	[AttributeUsage(AttributeTargets.Property)]
	private class UnfortunateAttribute : Attribute
	{
	}

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
				("Property1", new JsonSchemaBuilder().Ref("#/$defs/guid")),
				("Property2", new JsonSchemaBuilder().Ref("#/$defs/guid"))
			)
			.Defs(
				("guid", new JsonSchemaBuilder()
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
	public class MyAttribute1 : Attribute;

	[AttributeUsage(AttributeTargets.Property)]
	public class MyAttribute2 : Attribute;

	public class PersonRefiner : ISchemaRefiner
	{
		public Dictionary<string, List<Type>> FoundAttributes { get; } = [];

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

		Assert.Multiple(() =>
		{
			Assert.That(personRefiner.FoundAttributes[nameof(Person.FirstName)], Has.Count.EqualTo(1));
			Assert.That(personRefiner.FoundAttributes[nameof(Person.FirstName)][0], Is.EqualTo(typeof(MyAttribute1)));
			Assert.That(personRefiner.FoundAttributes[nameof(Person.LastName)], Has.Count.EqualTo(1));
			Assert.That(personRefiner.FoundAttributes[nameof(Person.LastName)][0], Is.EqualTo(typeof(MyAttribute2)));
		});
	}

	private class TypeWithSomeNullableOthersNot
	{
		[Nullable(true)]
		public string Nullable { get; set; }
		public string NotNullable { get; set; }
	}

	[Test]
	public void Issue325_NullableBleedingAcrossMembers()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Nullable", new JsonSchemaBuilder().Type(SchemaValueType.String | SchemaValueType.Null)),
				("NotNullable", new JsonSchemaBuilder().Type(SchemaValueType.String))
			);

		JsonSchema actual = new JsonSchemaBuilder().FromType<TypeWithSomeNullableOthersNot>();

		AssertEqual(expected, actual);
	}

	public class MyType450
	{
		[Nullable(true)]
		public IEnumerable<MyItem450> Items { get; set; }
	}

	public class MyItem450
	{
		[Nullable(true)]
		public IEnumerable<MyItem450> Items { get; set; }
	}

	[Test]
	public void Issue450_StackOverflowFromNestedCollections()
	{
		var schema = new JsonSchemaBuilder()
			.FromType<MyType450>()
			.Build();

		TestConsole.WriteLine(JsonSerializer.Serialize(schema, TestSerializerContext.Default.JsonSchema));
	}

	private class Issue512_Type
	{
		public IList<JsonNode> Foo { get; set; }
	}

	[Test]
	public void Issue512_IListOfJsonNodeThrows()
	{
		var schema = new JsonSchemaBuilder()
			.FromType<Issue512_Type>()
			.Build();

		TestConsole.WriteLine(JsonSerializer.Serialize(schema, TestSerializerContext.Default.JsonSchema));
	}

	private class Type544_ObsoleteProperties
	{
		[Obsolete]
		public int AAA { get; set; }

		public int BBB { get; set; }

		[Obsolete("CCC is deprecated.")]
		public int CCC { get; set; }
	}

	[Test]
	public void Issue544_DeprecatedSpillingOverToOtherPropertiesOfSameType()
	{
		var builder = new JsonSchemaBuilder();
		builder.FromType<Type544_ObsoleteProperties>();

		var schema = builder.Build();
		var schemaJson = JsonSerializer.Serialize(schema, TestSerializerContext.Default.JsonSchema);
		TestConsole.WriteLine(schemaJson);

		Assert.Multiple(() =>
		{
			Assert.That(schema.GetProperties()!["BBB"].Keywords!, Has.Count.EqualTo(1));
			Assert.That(schema.GetProperties()!["BBB"].Keywords!.First().Keyword(), Is.EqualTo("type"));
		});
	}

	private class Type551_MinItemsOnString
	{
		[MinItems(1)]
		[MaxItems(10)]
		public string Value { get; set; }
	}

	[Test]
	public void Issue551_MinMaxItemsOnStringProperty()
	{
		JsonSchema schema = new JsonSchemaBuilder().FromType<Type551_MinItemsOnString>();
		var schemaJson = JsonSerializer.Serialize(schema, TestSerializerContext.Default.JsonSchema);
		TestConsole.WriteLine(schemaJson);

		Assert.Multiple(() =>
		{
			Assert.That(schema.GetProperties()!["Value"].Keywords!, Has.Count.EqualTo(1));
			Assert.That(schema.GetProperties()!["Value"].Keywords!.First().Keyword(), Is.EqualTo("type"));
		});
	}

	private class Issue696_NullableDecimalWithMultipleOf
	{
		[Nullable(true)]
		[MultipleOf(0.1)]
		public decimal? Apr { get; set; }
	}

	[Test]
	public void Issue696_MultipleOfMissingForNullableDecimal()
	{
		var expected = JsonNode.Parse(
			"""
			{
			  "type": "object",
			  "properties": {
			    "Apr": {
			      "type": ["number", "null"],
			      "multipleOf": 0.1
			    }
			  }
			}
			""");

		JsonSchema schema = new JsonSchemaBuilder().FromType<Issue696_NullableDecimalWithMultipleOf>();
		var schemaJson = JsonSerializer.SerializeToNode(schema, TestSerializerContext.Default.JsonSchema);
		TestConsole.WriteLine(schemaJson);

		Assert.That(schemaJson.IsEquivalentTo(expected), Is.True);
	}

	public class Issue767_PropertyLevelComments
	{
		public class NestedType
		{
			/// <summary>
			/// Names property on NestedType
			/// </summary>
			public List<string> Names { get; set; } = [];

			/// <summary>
			/// Descriptions property on NestedType
			/// </summary>
			public List<string> Descriptions { get; set; } = [];

			public class NestedNestedType
			{
				/// <summary>
				/// NestedNames property on NestedNestedType (double-nested)
				/// </summary>
				public List<string> NestedNames { get; set; } = [];
			}

			/// <summary>
			/// NestedNested property on NestedType
			/// </summary>
			public NestedNestedType NestedNested = new();
		}

		/// <summary>
		/// Nested property on Issue767_PropertyLevelComments
		/// </summary>
		public NestedType? Nested = new();
	}


	[Test]
	public void Issue767_PropertyDescriptionFromXmlComments()
	{
		var expected = JsonNode.Parse(
			"""
			{
			  "type": "object",
			  "properties": {
			    "Nested": {
			      "type": "object",
			      "properties": {
			        "NestedNested": {
			          "$ref": "#/$defs/nestedNestedTypeInNestedTypeInIssue767PropertyLevelCommentsInClientTests"
			        },
			        "Names": {
			          "$ref": "#/$defs/listOfString1"
			        },
			        "Descriptions": {
			          "$ref": "#/$defs/listOfString2"
			        }
			      },
			      "description": "Nested property on Issue767_PropertyLevelComments"
			    }
			  },
			  "$defs": {
			    "nestedNestedTypeInNestedTypeInIssue767PropertyLevelCommentsInClientTests": {
			      "type": "object",
			      "properties": {
			        "NestedNames": {
			          "$ref": "#/$defs/listOfString"
			        }
			      },
			      "description": "NestedNested property on NestedType"
			    },
			    "listOfString": {
			      "type": "array",
			      "items": {
			        "type": "string",
			        "description": "NestedNames property on NestedNestedType (double-nested)"
			      },
			      "description": "NestedNames property on NestedNestedType (double-nested)"
			    },
			    "listOfString1": {
			      "type": "array",
			      "items": {
			        "type": "string",
			        "description": "Names property on NestedType"
			      },
			      "description": "Names property on NestedType"
			    },
			    "listOfString2": {
			      "type": "array",
			      "items": {
			        "type": "string",
			        "description": "Descriptions property on NestedType"
			      },
			      "description": "Descriptions property on NestedType"
			    }
			  }
			}
			""");

		var options = new SchemaGeneratorConfiguration { Optimize = true };
		options.RegisterXmlCommentFile<Issue767_PropertyLevelComments>("JsonSchema.Net.Generation.Tests.xml");
		JsonSchema schema = new JsonSchemaBuilder().FromType<Issue767_PropertyLevelComments>(options);
		var schemaJson = JsonSerializer.SerializeToNode(schema, TestSerializerContext.Default.JsonSchema);
		TestConsole.WriteLine(schemaJson);

		Assert.That(schemaJson.IsEquivalentTo(expected), Is.True);
	}

#if NET8_0_OR_GREATER
	private const string ExternalSchemaUri = "https://test.json-everything.net/has-external-schema";
	private const string GeneratedSchemaUri = "https://test.json-everything.net/uses-external-schema";

	[Id(ExternalSchemaUri)]
	internal class HasExternalSchemaUsingIdAttribute
	{
		public int Value { get; set; }
	}

	[Id(GeneratedSchemaUri)]
	internal class ShouldRefToExternalSchemaUsingIdAttributeWithRequiredKeyword
	{
		// it is this required keyword that prevents the $ref
		public required HasExternalSchemaUsingIdAttribute ShouldRef { get; set; }
	}

	[Id(GeneratedSchemaUri)]
	internal class ShouldRefToExternalSchemaUsingIdAttributeWithRequiredAttribute
	{
		[Required]
		public HasExternalSchemaUsingIdAttribute ShouldRef { get; set; }
	}

	[Test]
	public void Issue815_UsingCSharpRequiredKeyword()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Id(GeneratedSchemaUri)
			.Type(SchemaValueType.Object)
			.Properties(
				("ShouldRef", new JsonSchemaBuilder().Ref(ExternalSchemaUri))
			)
			.Required("ShouldRef");

		JsonSchema actual = new JsonSchemaBuilder().FromType<ShouldRefToExternalSchemaUsingIdAttributeWithRequiredKeyword>();

		AssertEqual(expected, actual);
	}

	[Test]
	public void Issue815_UsingRequiredAttribute()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Id(GeneratedSchemaUri)
			.Type(SchemaValueType.Object)
			.Properties(
				("ShouldRef", new JsonSchemaBuilder().Ref(ExternalSchemaUri))
			)
			.Required("ShouldRef");

		JsonSchema actual = new JsonSchemaBuilder().FromType<ShouldRefToExternalSchemaUsingIdAttributeWithRequiredKeyword>();

		AssertEqual(expected, actual);
	}
#endif
}