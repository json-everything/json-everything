using System;
using System.Threading.Tasks;
using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests;

public class StringGenerationTests
{
	[Test]
	public async Task SimpleString()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String);

		await Run(schema);
	}

	[Test]
	public async Task MinLength()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.MinLength(30);

		await Run(schema);
	}

	[Test]
	public async Task MaxLength()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.MaxLength(20);

		await Run(schema);
	}

	[Test]
	public async Task SpecifiedRange()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.MinLength(10)
			.MaxLength(20);

		await Run(schema);
	}

	[Test]
	[Ignore("regex not supported")]
	public async Task ContainsDog()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Pattern("dog");

		await Run(schema);
	}

	[Test]
	[Ignore("regex not supported")]
	public async Task ContainsDogWithSizeConstraints()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Pattern("dog")
			.MinLength(10)
			.MaxLength(20);

		await Run(schema);
	}

	[Test]
	[Ignore("regex not supported")]
	public async Task DoesNotContainDog()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Not(new JsonSchemaBuilder().Pattern("dog"));

		await Run(schema);
	}

	[Test]
	[Ignore("regex not supported")]
	public async Task DoesNotContainDogWithSizeConstraints()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Not(new JsonSchemaBuilder().Pattern("dog"))
			.MinLength(10)
			.MaxLength(20);

		await Run(schema);
	}

	[Test]
	[Ignore("not supported by regex lib")]
	public async Task ContainsCatAndDoesNotContainDogWithSizeConstraints()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Not(new JsonSchemaBuilder().Pattern("dog"))
			.Pattern("cat")
			.MinLength(10)
			.MaxLength(20);

		await Run(schema);
	}

	[Test]
	[Ignore("regex not supported")]
	public async Task ContainsEitherCatOrDog()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.AnyOf(
				new JsonSchemaBuilder().Pattern("dog"),
				new JsonSchemaBuilder().Pattern("cat")
			)
			.MinLength(10)
			.MaxLength(20);

		await Run(schema);
	}

	[Test]
	[Ignore("regex not supported")]
	public async Task ContainsExclusivelyEitherCatOrDog()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.OneOf(
				new JsonSchemaBuilder().Pattern("dog"),
				new JsonSchemaBuilder().Pattern("cat")
			)
			.MinLength(10)
			.MaxLength(20);

		await Run(schema);
	}

	[Test]
	public async Task FormatDate()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Format(Formats.Date);

		await Run(schema, new EvaluationOptions { RequireFormatValidation = true });
	}

	[Test]
	public async Task FormatDateTime()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Format(Formats.DateTime);

		await Run(schema, new EvaluationOptions { RequireFormatValidation = true });
	}

	[Test]
	public async Task FormatDuration()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Format(Formats.Duration);

		await Run(schema, new EvaluationOptions { RequireFormatValidation = true });
	}

	[Test]
	public async Task FormatEmail()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Format(Formats.Email);

		await Run(schema, new EvaluationOptions { RequireFormatValidation = true });
	}

	[Test]
	public async Task FormatHostname()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Format(Formats.Hostname);

		await Run(schema, new EvaluationOptions { RequireFormatValidation = true });
	}

	[Test]
	public async Task FormatIdnEmail()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Format(Formats.IdnEmail);

		await Run(schema, new EvaluationOptions { RequireFormatValidation = true });
	}

	[Test]
	public async Task FormatIdnHostname()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Format(Formats.IdnHostname);

		await Run(schema, new EvaluationOptions { RequireFormatValidation = true });
	}

	[Test]
	public async Task FormatIpv4()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Format(Formats.Ipv4);

		await Run(schema, new EvaluationOptions { RequireFormatValidation = true });
	}

	[Test]
	public async Task FormatIpv6()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Format(Formats.Ipv6);

		await Run(schema, new EvaluationOptions { RequireFormatValidation = true });
	}

	[Test]
	public async Task FormatIri()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Format(Formats.Iri);

		await Run(schema, new EvaluationOptions { RequireFormatValidation = true });
	}

	[Test]
	public async Task FormatIriReference()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Format(Formats.IriReference);

		await Run(schema, new EvaluationOptions { RequireFormatValidation = true });
	}

	[Test]
	public async Task FormatJsonPointer()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Format(Formats.JsonPointer);

		await Run(schema, new EvaluationOptions { RequireFormatValidation = true });
	}

	[Test]
	public async Task FormatRelativeJsonPointer()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Format(Formats.RelativeJsonPointer);

		await Run(schema, new EvaluationOptions { RequireFormatValidation = true });
	}

	[Test]
	public async Task FormatTime()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Format(Formats.Time);

		await Run(schema, new EvaluationOptions { RequireFormatValidation = true });
	}

	[Test]
	public async Task FormatUri()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Format(Formats.Uri);

		await Run(schema, new EvaluationOptions { RequireFormatValidation = true });
	}

	[Test]
	public async Task FormatUriReference()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Format(Formats.UriReference);

		await Run(schema, new EvaluationOptions { RequireFormatValidation = true });
	}

	[Test]
	public async Task FormatUuid()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Format(Formats.Uuid);

		await Run(schema, new EvaluationOptions { RequireFormatValidation = true });
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