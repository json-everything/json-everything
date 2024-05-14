using System;
using System.Text.Json;
using NUnit.Framework;

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

		JsonSchema schema = new JsonSchemaBuilder().FromType<TypeCommentedModel>(config);

		Assert.AreEqual("Type commented model description", schema.GetDescription());

		Assert.AreEqual("Type commented inner model description", schema.GetProperties()?["Inner"].GetDescription());
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

		JsonSchema schema = new JsonSchemaBuilder().FromType<MemberCommentedModel>(config);

		Assert.AreEqual("Bar is for counting", schema.GetProperties()?["Bar"].GetDescription());
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

		JsonSchema schema = new JsonSchemaBuilder().FromType<TypeAndMemberCommentedModel>(config);

		Assert.AreEqual("This overrides the type description", schema.GetProperties()?["Inner"].GetDescription());
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
		var config = new SchemaGeneratorConfiguration();
		config.RegisterXmlCommentFile<TypeAndSingleMemberCommentedModel>("JsonSchema.Net.Generation.Tests.xml");

		JsonSchema schema = new JsonSchemaBuilder().FromType<TypeAndSingleMemberCommentedModel>(config);

		Console.WriteLine(JsonSerializer.Serialize(schema, TestEnvironment.SerializerOptions));

		Assert.AreEqual("This overrides the type description", schema.GetProperties()?["Inner"].GetDescription());
		Assert.IsNull(schema.GetProperties()?["Inner2"].GetDescription());
		Assert.IsNull(schema.GetProperties()?["Inner3"].GetDescription());
		Assert.AreEqual("Type commented inner model description", schema.GetDefs()?["typeCommentedInnerModelInCommentsTests"].GetDescription());
	}
}