using NUnit.Framework;

namespace Json.Schema.Generation.Tests;

public class ExternalReferenceTests
{
	private const string ExternalSchemaUri = "https://test.json-everything.net/has-external-schema";
	private const string GeneratedSchemaUri = "https://test.json-everything.net/uses-external-schema";

	internal class HasExternalSchema
	{
		public int Value { get; set; }
	}

	internal class ShouldRefToExternalSchema
	{
		public HasExternalSchema ShouldRef { get; set; }
	}

	[Test]
	public void GeneratesRefForExternalReference()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("ShouldRef", new JsonSchemaBuilder().Ref(ExternalSchemaUri))
			);

		var config = new SchemaGeneratorConfiguration
		{
			ExternalReferences =
			{
				[typeof(HasExternalSchema)] = new(ExternalSchemaUri)
			}
		};

		JsonSchema actual = new JsonSchemaBuilder().FromType<ShouldRefToExternalSchema>(config);

		AssertionExtensions.AssertEqual(expected, actual);
	}

	internal class RefWithAttributes
	{
		[Required]
		[Title("this one has attributes")]
		public HasExternalSchema ShouldRef { get; set; }
	}


	[Test]
	public void GeneratesRefForExternalReferenceWithAttributes()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("ShouldRef", new JsonSchemaBuilder()
					.Ref(ExternalSchemaUri)
					.Title("this one has attributes")
				)
			)
			.Required("ShouldRef");

		var config = new SchemaGeneratorConfiguration
		{
			ExternalReferences =
			{
				[typeof(HasExternalSchema)] = new(ExternalSchemaUri)
			}
		};

		JsonSchema actual = new JsonSchemaBuilder().FromType<RefWithAttributes>(config);

		AssertionExtensions.AssertEqual(expected, actual);
	}

	[Id(ExternalSchemaUri)]
	internal class HasExternalSchemaUsingIdAttribute
	{
		public int Value { get; set; }
	}

	[Id(GeneratedSchemaUri)]
	internal class ShouldRefToExternalSchemaUsingIdAttribute
	{
		public HasExternalSchemaUsingIdAttribute ShouldRef { get; set; }
	}

	[Test]
	public void GeneratesRefForExternalReferenceUsingIdAttribute()
	{
		JsonSchema expected = new JsonSchemaBuilder(new BuildOptions { SchemaRegistry = new() })
			.Id(GeneratedSchemaUri)
			.Type(SchemaValueType.Object)
			.Properties(
				("ShouldRef", new JsonSchemaBuilder().Ref(ExternalSchemaUri))
			);

		JsonSchema actual = new JsonSchemaBuilder(new BuildOptions{SchemaRegistry = new()}).FromType<ShouldRefToExternalSchemaUsingIdAttribute>();

		AssertionExtensions.AssertEqual(expected, actual);
	}
}