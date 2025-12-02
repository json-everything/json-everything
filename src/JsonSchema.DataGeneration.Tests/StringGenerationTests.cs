using System;
using NUnit.Framework;
using TestHelpers;
using static Json.Schema.DataGeneration.Tests.TestRunner;

namespace Json.Schema.DataGeneration.Tests;

public class StringGenerationTests
{
	[Test]
	public void SimpleString()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String);

		Run(schema, buildOptions);
	}

	[Test]
	public void MinLength()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.MinLength(30);

		Run(schema, buildOptions);
	}

	[Test]
	public void MaxLength()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.MaxLength(20);

		Run(schema, buildOptions);
	}

	[Test]
	public void SpecifiedRange()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.MinLength(10)
			.MaxLength(20);

		Run(schema, buildOptions);
	}

	[Test]
	public void ContainsDog()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Pattern("dog");

		Run(schema, buildOptions);
	}

	[Test]
	public void ContainsDogWithSizeConstraints()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Pattern("dog")
			.MinLength(10)
			.MaxLength(20);

		Assert.Throws<NotSupportedException>(() => Run(schema, buildOptions));
	}

	[Test]
	public void DoesNotContainDog()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Not(new JsonSchemaBuilder().Pattern("dog"));

		Assert.Throws<NotSupportedException>(() => Run(schema, buildOptions));
	}

	[Test]
	public void DoesNotContainDogWithSizeConstraints()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Not(new JsonSchemaBuilder().Pattern("dog"))
			.MinLength(10)
			.MaxLength(20);

		Assert.Throws<NotSupportedException>(() => Run(schema, buildOptions));
	}

	[Test]
	public void ContainsCatAndDoesNotContainDogWithSizeConstraints()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Not(new JsonSchemaBuilder().Pattern("dog"))
			.Pattern("cat")
			.MinLength(10)
			.MaxLength(20);

		Assert.Throws<NotSupportedException>(() => Run(schema, buildOptions));
	}

	[Test]
	public void ContainsEitherCatOrDog()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.AnyOf(
				new JsonSchemaBuilder().Pattern("dog"),
				new JsonSchemaBuilder().Pattern("cat")
			)
			.MinLength(10)
			.MaxLength(20);

		Assert.Throws<NotSupportedException>(() => Run(schema, buildOptions));
	}

	[Test]
	public void ContainsExclusivelyEitherCatOrDog()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.OneOf(
				new JsonSchemaBuilder().Pattern("dog"),
				new JsonSchemaBuilder().Pattern("cat")
			)
			.MinLength(10)
			.MaxLength(20);

		Assert.Throws<NotSupportedException>(() => Run(schema, buildOptions));
	}

	[Test]
	public void FormatDate()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.Date);

		Run(schema, buildOptions);
	}

	[Test]
	public void FormatDateTime()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.DateTime);

		Run(schema, buildOptions);
	}

	[Test]
	public void FormatDuration()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.Duration);

		Run(schema, buildOptions);
	}

	[Test]
	public void FormatEmail()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.Email);

		Run(schema, buildOptions);
	}

	[Test]
	public void FormatHostname()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.Hostname);

		Run(schema, buildOptions);
	}

	[Test]
	public void FormatIdnEmail()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.IdnEmail);

		Run(schema, buildOptions);
	}

	[Test]
	public void FormatIdnHostname()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.IdnHostname);

		Run(schema, buildOptions);
	}

	[Test]
	public void FormatIpv4()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.Ipv4);

		Run(schema, buildOptions);
	}

	[Test]
	public void FormatIpv6()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.Ipv6);

		Run(schema, buildOptions);
	}

	[Test]
	public void FormatIri()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.Iri);

		Run(schema, buildOptions);
	}

	[Test]
	public void FormatIriReference()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Format(Formats.IriReference);

		Run(schema, buildOptions);
	}

	[Test]
	public void FormatJsonPointer()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.JsonPointer);

		Run(schema, buildOptions);
	}

	[Test]
	public void FormatRelativeJsonPointer()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.RelativeJsonPointer);

		Run(schema, buildOptions);
	}

	[Test]
	public void FormatTime()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.Time);

		Run(schema, buildOptions);
	}

	[Test]
	public void FormatUri()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.Uri);

		Run(schema, buildOptions);
	}

	[Test]
	public void FormatUriReference()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.UriReference);

		Run(schema, buildOptions);
	}

	[Test]
	public void FormatUuid()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.Uuid);

		Run(schema, buildOptions);
	}

	[Test]
	public void MultipleFormats()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.AllOf(
				new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.Format(Formats.DateTime),
				new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.Format(Formats.Uuid)
			);

		var result = schema.GenerateData(buildOptions);

		TestConsole.WriteLine(result.ErrorMessage);
		Assert.That(result.IsSuccess, Is.False);
	}
}