using System;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests
{
	public class FormatTests
	{
		[Test]
		public void Ipv4_Pass()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Format(Formats.Ipv4);

			var value = JsonDocument.Parse("\"100.2.54.3\"");

			var result = schema.Validate(value.RootElement, new ValidationOptions{RequireFormatValidation = true});

			Assert.True(result.IsValid);
		}
		[Test]
		public void Ipv4_Fail()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Format(Formats.Ipv4);

			var value = JsonDocument.Parse("\"100.2.5444.3\"");

			var result = schema.Validate(value.RootElement, new ValidationOptions {RequireFormatValidation = true});

			Assert.False(result.IsValid);
		}

		private static readonly Uri _formatAssertionMetaSchemaId = new Uri("https://json-everything/test/format-assertion");
		private static readonly JsonSchema _formatAssertionMetaSchema =
						new JsonSchemaBuilder()
				.Schema(MetaSchemas.Draft202012Id)
				.Id(_formatAssertionMetaSchemaId)
				.Vocabulary(
					(Vocabularies.Core202012Id, true),
					(Vocabularies.Applicator202012Id, true),
					(Vocabularies.Metadata202012Id, true),
					(Vocabularies.FormatAssertion202012Id, false)
				)
				.DynamicAnchor("meta")
				.Title("format assertion meta-schema")
				.AllOf(
					new JsonSchemaBuilder().Ref(MetaSchemas.Core202012Id),
					new JsonSchemaBuilder().Ref(MetaSchemas.Applicator202012Id),
					new JsonSchemaBuilder().Ref(MetaSchemas.Metadata202012Id),
					new JsonSchemaBuilder().Ref(MetaSchemas.FormatAssertion202012Id)
				)
				.Type(SchemaValueType.Object | SchemaValueType.Boolean);

		[Test]
		public void UnknownFormat_Annotation_ReportsFormat()
		{
			var schemaText = $@"{{
	""$schema"": ""{MetaSchemas.Draft202012Id}"",
	""type"": ""string"",
	""format"": ""something-dumb""
}}";
			var schema = JsonSchema.FromText(schemaText);
			var instance = JsonDocument.Parse("\"a value\"").RootElement;

			var results = schema.Validate(instance, new ValidationOptions{OutputFormat = OutputFormat.Detailed});

			results.AssertValid();
			var serialized = JsonSerializer.Serialize(results);
			Assert.IsTrue(serialized.Contains("something-dumb"));
		}

		[Test]
		public void UnknownFormat_Assertion_FailsValidation()
		{
			var options = new ValidationOptions
            {
                OutputFormat = OutputFormat.Detailed,
				OnlyKnownFormats = true
            };
			options.SchemaRegistry.Register(_formatAssertionMetaSchemaId, _formatAssertionMetaSchema);

			var schemaText = $@"{{
	""$schema"": ""{_formatAssertionMetaSchemaId}"",
	""type"": ""string"",
	""format"": ""something-dumb""
}}";
			var schema = JsonSchema.FromText(schemaText);
			var instance = JsonDocument.Parse("\"a value\"").RootElement;

			var results = schema.Validate(instance, options);

			results.AssertInvalid();
			var serialized = JsonSerializer.Serialize(results);
			Assert.IsTrue(serialized.Contains("something-dumb"));
		}

		[Test]
		public void UnknownFormat_AnnotationWithAssertionOption_FailsValidation()
		{
			var schemaText = $@"{{
	""$schema"": ""{MetaSchemas.Draft202012Id}"",
	""type"": ""string"",
	""format"": ""something-dumb""
}}";
			var schema = JsonSchema.FromText(schemaText);
			var instance = JsonDocument.Parse("\"a value\"").RootElement;

			var results = schema.Validate(instance, new ValidationOptions
			{
				OutputFormat = OutputFormat.Detailed,
				RequireFormatValidation = true,
                OnlyKnownFormats = true
			});

			results.AssertInvalid();
			var serialized = JsonSerializer.Serialize(results);
			Assert.IsTrue(serialized.Contains("something-dumb"));
		}
	}
}