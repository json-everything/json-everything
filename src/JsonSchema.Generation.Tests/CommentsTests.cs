using System.Text.Json;
using Json.More;
using NUnit.Framework;
using TestHelpers;
using static Json.Schema.Generation.Tests.AssertionExtensions;

namespace Json.Schema.Generation.Tests;

public class CommentsTests
{
	/// <summary>
	/// Type commented model description
	/// </summary>
	private class TypeCommentedModel
	{
		public int Foo { get; set; }
		public TypeCommentedInnerModel Inner { get; set; }
	}

	/// <summary>
	/// Type commented inner model description
	/// </summary>
	private class TypeCommentedInnerModel
	{
		public int Bar { get; set; }
	}

	[Test]
	public void TypeCommentsAreAdded()
	{
		var config = new SchemaGeneratorConfiguration();
		config.RegisterXmlCommentFile<TypeCommentedModel>("JsonSchema.Net.Generation.Tests.xml");

		var builder = new JsonSchemaBuilder();
		builder.FromType<TypeCommentedModel>(config);

		var schema = builder.Build();
		TestConsole.WriteLine(schema.Root.Source);

		var expected = JsonDocument.Parse(
			"""
			{
			  "type": "object",
			  "description": "Type commented model description",
			  "properties": {
			    "Foo": {"type": "integer"},
			    "Inner": {
			      "type": "object",
			      "description": "Type commented inner model description",
			      "properties": {
			        "Bar": {"type": "integer"}
			      }
			    }
			  }
			}
			""").RootElement;
	
		Assert.That(expected.IsEquivalentTo(schema.Root.Source));
	}

	private class MemberCommentedModel
	{
		/// <summary>
		/// Bar is for counting
		/// </summary>
		public int Bar { get; set; }
	}

	[Test]
	public void MemberCommentsAreAdded()
	{
		var config = new SchemaGeneratorConfiguration();
		config.RegisterXmlCommentFile<MemberCommentedModel>("JsonSchema.Net.Generation.Tests.xml");

		var builder = new JsonSchemaBuilder();
		builder.FromType<MemberCommentedModel>(config);

		var schema = builder.Build();
		TestConsole.WriteLine(schema.Root.Source);

		var expected = JsonDocument.Parse(
			"""
			{
			  "type": "object",
			  "properties": {
			    "Bar": {
			      "type": "integer",
			      "description": "Bar is for counting"
			    }
			  }
			}
			""").RootElement;
	
		Assert.That(expected.IsEquivalentTo(schema.Root.Source));
	}

	/// <summary>
	/// Type commented model description
	/// </summary>
	private class TypeAndMemberCommentedModel
	{
		/// <summary>
		/// This overrides the type description
		/// </summary>
		public TypeCommentedInnerModel Inner { get; set; }
	}

	[Test]
	public void MemberCommentOverridesTypeComment()
	{
		var config = new SchemaGeneratorConfiguration();
		config.RegisterXmlCommentFile<TypeAndMemberCommentedModel>("JsonSchema.Net.Generation.Tests.xml");

		var builder = new JsonSchemaBuilder();
		builder.FromType<TypeAndMemberCommentedModel>(config);

		var schema = builder.Build();
		TestConsole.WriteLine(schema.Root.Source);

		var expected = JsonDocument.Parse(
			"""
			{
			  "description": "Type commented model description",
			  "type": "object",
			  "properties": {
			    "Inner": {
			      "$ref": "#/$defs/typeCommentedInnerModelInCommentsTests",
			      "description": "This overrides the type description"
			    }
			  },
			  "$defs": {
			    "typeCommentedInnerModelInCommentsTests": {
			      "description": "Type commented inner model description",
			      "type": "object",
			      "properties": {
			        "Bar": {
			          "type": "integer"
			        }
			      }
			    }
			  }
			}
			""").RootElement;
	
		Assert.That(expected.IsEquivalentTo(schema.Root.Source));
	}

	/// <summary>
	/// Type commented model description
	/// </summary>
	private class TypeAndSingleMemberCommentedModel
	{
		/// <summary>
		/// This overrides the type description
		/// </summary>
		public TypeCommentedInnerModel Inner { get; set; }
		public TypeCommentedInnerModel Inner2 { get; set; }
		public TypeCommentedInnerModel Inner3 { get; set; }
	}

	[Test]
	public void DefinitionContainsTypeCommentAndRefContainsMemberComment()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Description("Type commented model description")
			.Properties(
				("Inner", new JsonSchemaBuilder()
					.Ref("#/$defs/typeCommentedInnerModelInCommentsTests")
					.Description("This overrides the type description")
				),
				("Inner2", new JsonSchemaBuilder()
					.Ref("#/$defs/typeCommentedInnerModelInCommentsTests")
				),
				("Inner3", new JsonSchemaBuilder()
					.Ref("#/$defs/typeCommentedInnerModelInCommentsTests")
				)
			)
			.Defs(
				("typeCommentedInnerModelInCommentsTests", new JsonSchemaBuilder()
					.Type(SchemaValueType.Object)
					.Description("Type commented inner model description")
					.Properties(
						("Bar", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
					)
				)
			);

		var config = new SchemaGeneratorConfiguration();
		config.RegisterXmlCommentFile<TypeAndSingleMemberCommentedModel>("JsonSchema.Net.Generation.Tests.xml");

		JsonSchema schema = new JsonSchemaBuilder().FromType<TypeAndSingleMemberCommentedModel>(config);

		AssertEqual(expected, schema);
	}
}