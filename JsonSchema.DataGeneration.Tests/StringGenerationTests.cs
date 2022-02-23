using System;
using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests
{
	public class StringGenerationTests
	{
		[Test]
		public void SimpleString()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String);

			Run(schema);
		}

		[Test]
		public void MinLength()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.MinLength(30);

			Run(schema);
		}

		[Test]
		public void MaxLength()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.MaxLength(20);

			Run(schema);
		}

		[Test]
		public void SpecifiedRange()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.MinLength(10)
				.MaxLength(20);

			Run(schema);
		}

		[Test]
		[Ignore("regex not supported")]
		public void ContainsDog()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Pattern("dog");

			Run(schema);
		}

		[Test]
		[Ignore("regex not supported")]
		public void ContainsDogWithSizeConstraints()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Pattern("dog")
				.MinLength(10)
				.MaxLength(20);

			Run(schema);
		}

		[Test]
		[Ignore("regex not supported")]
		public void DoesNotContainDog()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Not(new JsonSchemaBuilder().Pattern("dog"));

			Run(schema);
		}

		[Test]
		[Ignore("regex not supported")]
		public void DoesNotContainDogWithSizeConstraints()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Not(new JsonSchemaBuilder().Pattern("dog"))
				.MinLength(10)
				.MaxLength(20);

			Run(schema);
		}

		[Test]
		[Ignore("not supported by regex lib")]
		public void ContainsCatAndDoesNotContainDogWithSizeConstraints()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Not(new JsonSchemaBuilder().Pattern("dog"))
				.Pattern("cat")
				.MinLength(10)
				.MaxLength(20);

			Run(schema);
		}

		[Test]
		[Ignore("regex not supported")]
		public void ContainsEitherCatOrDog()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.AnyOf(
					new JsonSchemaBuilder().Pattern("dog"),
					new JsonSchemaBuilder().Pattern("cat")
				)
				.MinLength(10)
				.MaxLength(20);

			Run(schema);
		}

		[Test]
		[Ignore("regex not supported")]
		public void ContainsExclusivelyEitherCatOrDog()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.OneOf(
					new JsonSchemaBuilder().Pattern("dog"),
					new JsonSchemaBuilder().Pattern("cat")
				)
				.MinLength(10)
				.MaxLength(20);

			Run(schema);
		}

		[Test]
		public void FormatDate()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Format(Formats.Date);

			Run(schema, new ValidationOptions {RequireFormatValidation = true});
		}

		[Test]
		public void FormatDateTime()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Format(Formats.DateTime);

			Run(schema, new ValidationOptions {RequireFormatValidation = true});
		}

		[Test]
		public void FormatDuration()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Format(Formats.Duration);

			Run(schema, new ValidationOptions {RequireFormatValidation = true});
		}

		[Test]
		public void FormatEmail()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Format(Formats.Email);

			Run(schema, new ValidationOptions {RequireFormatValidation = true});
		}

		[Test]
		public void FormatHostname()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Format(Formats.Hostname);

			Run(schema, new ValidationOptions {RequireFormatValidation = true});
		}

		[Test]
		public void FormatIdnEmail()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Format(Formats.IdnEmail);

			Run(schema, new ValidationOptions {RequireFormatValidation = true});
		}

		[Test]
		public void FormatIdnHostname()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Format(Formats.IdnHostname);

			Run(schema, new ValidationOptions {RequireFormatValidation = true});
		}

		[Test]
		public void FormatIpv4()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Format(Formats.Ipv4);

			Run(schema, new ValidationOptions {RequireFormatValidation = true});
		}

		[Test]
		public void FormatIpv6()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Format(Formats.Ipv6);

			Run(schema, new ValidationOptions {RequireFormatValidation = true});
		}

		[Test]
		public void FormatIri()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Format(Formats.Iri);

			Run(schema, new ValidationOptions {RequireFormatValidation = true});
		}

		[Test]
		public void FormatIriReference()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Format(Formats.IriReference);

			Run(schema, new ValidationOptions {RequireFormatValidation = true});
		}

		[Test]
		public void FormatJsonPointer()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Format(Formats.JsonPointer);

			Run(schema, new ValidationOptions {RequireFormatValidation = true});
		}

		[Test]
		public void FormatRelativeJsonPointer()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Format(Formats.RelativeJsonPointer);

			Run(schema, new ValidationOptions {RequireFormatValidation = true});
		}

		[Test]
		public void FormatTime()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Format(Formats.Time);

			Run(schema, new ValidationOptions {RequireFormatValidation = true});
		}

		[Test]
		public void FormatUri()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Format(Formats.Uri);

			Run(schema, new ValidationOptions {RequireFormatValidation = true});
		}

		[Test]
		public void FormatUriReference()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Format(Formats.UriReference);

			Run(schema, new ValidationOptions {RequireFormatValidation = true});
		}

		[Test]
		public void FormatUuid()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Format(Formats.Uuid);

			Run(schema, new ValidationOptions {RequireFormatValidation = true});
		}

		[Test]
		public void MultipleFormats()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.AllOf(
					new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
						.Format(Formats.DateTime),
					new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
						.Format(Formats.Uuid)
				);

			var result = schema.GenerateData();

			Console.WriteLine(result.ErrorMessage);
			Assert.IsFalse(result.IsSuccess);
		}
	}
}