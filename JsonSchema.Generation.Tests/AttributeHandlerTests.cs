using System;
using NUnit.Framework;
using static Json.Schema.Generation.Tests.AssertionExtensions;

namespace Json.Schema.Generation.Tests;

public class AttributeHandlerTests
{
	[AttributeUsage(AttributeTargets.Property)]
	private class AttributeWithDirectHandler : Attribute, IAttributeHandler
	{
		public const uint MaxLength = 100;

		void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
		{
			context.Intents.Add(new Intents.MaxLengthIntent(MaxLength));
		}
	}

	[AttributeUsage(AttributeTargets.Property)]
	private class AttributeWithIndirectHandler : Attribute
	{
		public readonly uint MaxLength = 200;
	}

	private class CustomAttributeHandler : IAttributeHandler<AttributeWithIndirectHandler>
	{
		void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
		{
			var maxLength = attribute as AttributeWithIndirectHandler;
			context.Intents.Add(new Intents.MaxLengthIntent(maxLength.MaxLength));
		}
	}

	private class TypeWithCustomattribute
	{
		[AttributeWithDirectHandler]
		public string MyProperty { get; set; }
	}

	private class TypeWithCustomAttribute2
	{
		[AttributeWithIndirectHandler]
		public string MyProperty { get; set; }
	}

	[Test]
	public void DirectAttributeHandler()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("MyProperty", new JsonSchemaBuilder().Type(SchemaValueType.String).MaxLength(AttributeWithDirectHandler.MaxLength))
			);

		JsonSchema actual = new JsonSchemaBuilder().FromType<TypeWithCustomattribute>();

		AssertEqual(expected, actual);
	}

	[Test]
	public void IndirectAttributeHandler()
	{
		AttributeHandler.RemoveHandler<CustomAttributeHandler>();
		AttributeHandler.AddHandler<CustomAttributeHandler>();

		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("MyProperty", new JsonSchemaBuilder().Type(SchemaValueType.String).MaxLength(200))
			);

		JsonSchema actual = new JsonSchemaBuilder().FromType<TypeWithCustomAttribute2>();

		AssertEqual(expected, actual);
	}
}